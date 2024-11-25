using System.Reflection;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Solhigson.Framework.Extensions;
using SolhigsonAspnetCoreApp.Application.Services.Abstractions;
using SolhigsonAspnetCoreApp.Application.Services.Logs;
using SolhigsonAspnetCoreApp.Infrastructure;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;
using Module = Autofac.Module;

namespace SolhigsonAspnetCoreApp.Application;

public class AutofacModule : Module
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;

    public AutofacModule(IConfiguration configuration, IHostEnvironment webHostEnvironment)
    {
        _configuration = configuration;
        _env = webHostEnvironment;
    }

    protected override void Load(ContainerBuilder builder)
    {
        #region Registed AsSelf(), no interface implementation
        
        builder.Register(c =>
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>();
            opt.UseSqlServer(_configuration.DefaultConnectionString());
            return new AppDbContext(opt.Options);
        }).AsSelf().InstancePerLifetimeScope();

        // builder.Register(c =>
        // {
        //     return database is null || string.IsNullOrWhiteSpace(auditLogContainerName)
        //         ? new NotSupportedAuditLogService() as IAuditLogService
        //         : new AuditLogService(database.Client, database.Id,
        //             _env.GetContainerOrCollectionName(auditLogContainerName));
        // }).As<IAuditLogService>().SingleInstance();

        builder.RegisterIndicatedDependencies(Assembly.GetAssembly(typeof(ApplicationAssemblyReference)));
        builder.RegisterIndicatedDependencies(Assembly.GetAssembly(typeof(InfrastructureAssemblyReference)));

        #endregion

        builder.RegisterType<RepositoryWrapper>().As<IRepositoryWrapper>().InstancePerLifetimeScope()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        base.Load(builder);
    }
}