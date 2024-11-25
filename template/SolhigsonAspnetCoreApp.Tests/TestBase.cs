using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Session;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NSubstitute;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Infrastructure;
using Solhigson.Framework.Mocks;
using Solhigson.Framework.Persistence;
using Solhigson.Framework.Web.Api;
using SolhigsonAspnetCoreApp.Application;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Application.Web;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Infrastructure;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Extensions = SolhigsonAspnetCoreApp.ServiceDefaults.Extensions;

namespace SolhigsonAspnetCoreApp.Tests;

//This file is never overwritten, place custom code here
public class TestBase : IDisposable
{
    private DbConnection _connection;

    public TestBase(ITestOutputHelper testOutputHelper)
    {
        testOutputHelper.ConfigureNLogConsoleOutputTarget();
        var builder = new ContainerBuilder();

        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        RegisterStartUpDependencies(builder, services, configuration, new TestHostEnvironment());

        /*
         * Override certain dependencies for mocking
         */
        //IConfiguration
        builder.RegisterInstance(new ConfigurationBuilder().Build()).As<IConfiguration>().SingleInstance();

        //Solhigson.Framework IApiRequestService
        builder.RegisterInstance(Substitute.For<IApiRequestService>()).SingleInstance();

        //Hangfire IBackgroundJobClient
        builder.RegisterType<MockHangfireBackgroundJobClient>().As<IBackgroundJobClient>()
            .SingleInstance();

        //AppDbContext to use EfCore in sqlite in memory database
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        builder.Register(c =>
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>();
            opt.UseSqlite(_connection);
            return new AppDbContext(opt.Options);
        }).AsSelf().InstancePerLifetimeScope();

        var opt = new DbContextOptionsBuilder<SolhigsonDbContext>();
        opt.UseSqlite(_connection);
        SolhigsonAutofacModule.LoadDbSupport(builder, configuration, opt);
        builder.Register(c => new SolhigsonDbContext(opt.Options)).AsSelf().InstancePerLifetimeScope();

        //Any other custom dependency overrides - implementation in BaseTest.cs
        LoadCustomDependencyOverrides(builder, services, configuration);

        builder.Populate(services);
        ServiceProvider = new AutofacServiceProvider(builder.Build());

        ServicesWrapper = ServiceProvider.GetRequiredService<ServicesWrapper>();

        // *** For Arrange and Assert only!!! ***
        RepositoryWrapper = ServiceProvider.GetRequiredService<IRepositoryWrapper>();

        //Ensure sqlite db is refreshed
        var dbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var solhigsonDbContext = ServiceProvider.GetRequiredService<SolhigsonDbContext>();
        solhigsonDbContext.Database.ExecuteSqlRaw(solhigsonDbContext.Database.GenerateCreateScript());

        Extensions.ConfigureDefaults(ServicesWrapper);

        InitializeSeedData().Wait();
    }

    protected IServiceProvider ServiceProvider { get; }

    protected ServicesWrapper ServicesWrapper { get; }

    // *** For Arrange and Assert only!!! ***
    protected IRepositoryWrapper RepositoryWrapper { get; }


    public void Dispose()
    {
        if (_connection == null) return;
        _connection.Dispose();
        _connection = null;
        GC.SuppressFinalize(this);
    }

    private void RegisterStartUpDependencies(ContainerBuilder builder, IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        builder.RegisterInstance(Substitute.For<IUrlHelper>()).SingleInstance();
        builder.RegisterSolhigsonDependencies(configuration, configuration.DefaultConnectionString());
        builder.RegisterModule(new AutofacModule(configuration, environment));

        builder.RegisterInstance(Substitute.For<IWebHostEnvironment>()).SingleInstance();
        builder.RegisterInstance(Substitute
            .For<IActionDescriptorCollectionProvider>()).SingleInstance();

        services.AddLogging();
        services.AddSolhigsonIdentityManager<AppUser, AppDbContext>(option =>
        {
            option.Password.RequireDigit = false;
            option.Password.RequireLowercase = false;
            option.Password.RequireNonAlphanumeric = false;
            option.Password.RequireUppercase = false;
            option.Lockout.AllowedForNewUsers = false;
            option.Lockout.MaxFailedAccessAttempts = 5;
        });
        services.AddMemoryCache();
    }

    private static void LoadCustomDependencyOverrides(ContainerBuilder builder, IServiceCollection services,
        IConfiguration configuration)
    {
    }

    private async Task InitializeSeedData()
    {
        var seederService = ServiceProvider.GetRequiredService<SeederService>();
        seederService.Seed(null).Wait();

        var inst = new InstitutionDto
        {
            Name = "Test Institution"
        };
        var res = await ServicesWrapper.ConfigService.CreateOrEditInstitutionAsync(inst);
        if (!res.IsSuccessful) throw new Exception(res.Message);
    }

    protected Institution GetDefaultInstitution()
    {
        return RepositoryWrapper.DbContext.Institutions.FirstOrDefault();
    }

    protected ActionExecutingContext GetActionExecutingContext<T>(T controller = null) where T : ControllerBase
    {
        controller ??= GetController<T>();
        var actionExecutingContext = new ActionExecutingContext(controller.ControllerContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            controller);
        Assert.NotNull(actionExecutingContext.HttpContext);
        Assert.NotNull(actionExecutingContext.HttpContext.Request);
        return actionExecutingContext;
    }

    private T GetController<T>() where T : ControllerBase
    {
        var memCache = new MemoryDistributedCache(new OptionsManager<MemoryDistributedCacheOptions>(
            new OptionsFactory<MemoryDistributedCacheOptions>(
                new List<IConfigureOptions<MemoryDistributedCacheOptions>>(),
                new List<IPostConfigureOptions<MemoryDistributedCacheOptions>>(),
                new List<IValidateOptions<MemoryDistributedCacheOptions>>())));
        var httpContext = new DefaultHttpContext
        {
            Session = new DistributedSession(memCache, "test", TimeSpan.MaxValue, TimeSpan.MaxValue,
                () => true, new NLogLoggerFactory(), true),
            RequestServices = ServiceProvider
        };
        var actionContext =
            new ActionContext(httpContext,
                new RouteData(),
                new ControllerActionDescriptor());
        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
        var controller = ServiceProvider.GetRequiredService<T>();
        controller.ControllerContext = new ControllerContext(actionContext);
        return controller;
    }

    protected T GetMvcController<T>() where T : MvcBaseController
    {
        var controller = GetController<T>();
        controller.TempData = new TempDataDictionary(controller.HttpContext, Substitute.For<ITempDataProvider>());
        var userService = ServiceProvider.GetRequiredService<UserService>();
        var institution = GetDefaultInstitution();
        var user = new SessionUser
        {
            Id = Guid.NewGuid().ToString(),
            InstitutionId = institution.Id,
            Institution = RepositoryWrapper.InstitutionRepository.GetByIdCachedAsync(institution.Id).Result,
            Firstname = "Test",
            Lastname = "User",
            Email = "testuser@test.com",
            Role = new SolhigsonAspNetRole
            {
                Name = "Test Role"
            },
            UserName = "testuser@test.com"
        };
        userService.SetCurrentSessionUser(user);
        controller.SetClaimsPrincipal(user);
        controller.OnActionExecuting(GetActionExecutingContext(controller));
        return controller;
    }
}