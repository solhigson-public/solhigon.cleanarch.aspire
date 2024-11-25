using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Audit.Core;
using Audit.MongoDB.Providers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Mapster;
using MassTransit;
using MassTransit.NewIdProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Solhigson.Framework.EfCore;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Logging.Nlog;
using Solhigson.Framework.Web.Hangfire;
using Solhigson.Framework.Web.Middleware;
using Solhigson.Framework.Web.Swagger;
using SolhigsonAspnetCoreApp.Application;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Application.Services.MongoDb;
using SolhigsonAspnetCoreApp.Application.Web.Filters;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Domain.MongoDb;
using SolhigsonAspnetCoreApp.Infrastructure;
using StackExchange.Redis;
using LogLevel = NLog.LogLevel;

namespace SolhigsonAspnetCoreApp.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private static readonly LogWrapper Logger = LogManager.GetLogger(typeof(Extensions).FullName);

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();
        
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });
        
        builder.AddRedisClient("redisCache");
        builder.AddRedisDistributedCache("redisCache");
        builder.AddMongoDBClient("SolhigsonAspnetCoreApp");

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRequestTimeouts(
            configure: static timeouts =>
                timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5)));

        builder.Services.AddOutputCache(
            configureOptions: static caching =>
                caching.AddPolicy("HealthChecks",
                    build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));
        
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }
    
    public static void RegisterDependencies(this ContainerBuilder containerBuilder, IHostApplicationBuilder builder, Assembly webAssembly)
    {
        var connectionString = builder.Configuration.DefaultConnectionString();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            containerBuilder.RegisterSolhigsonDependencies(builder.Configuration, connectionString);
        }
        containerBuilder.RegisterModule(new AutofacModule(builder.Configuration, builder.Environment));

        containerBuilder.RegisterAssemblyTypes(webAssembly)
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
            .InstancePerLifetimeScope()
            .PropertiesAutowired();
    }

    public static WebApplicationBuilder ConfigureCommonServices(this WebApplicationBuilder builder, Assembly webAssembly)
    {
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureLogging(action =>
            {
                action.ClearProviders();
                action.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog();
        
        builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            containerBuilder.RegisterDependencies(builder, webAssembly);
        });
        
        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddControllers(options =>
            options.Filters.Add(new HttpResponseExceptionFilter()));

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        AddHangfireService(services, builder.Configuration);

        services.AddSolhigsonIdentityManager<AppUser, AppDbContext>(option =>
        {
            option.Password.RequireDigit = false;
            option.Password.RequireLowercase = false;
            option.Password.RequireNonAlphanumeric = false;
            option.Password.RequireUppercase = false;
            option.Lockout.AllowedForNewUsers = false;
            option.Lockout.MaxFailedAccessAttempts = 5;
        });

        if (!int.TryParse(builder.Configuration["appSettings:UserTokenValidityPeriodHrs"], out var userTokenValidityPeriodHrs))
            userTokenValidityPeriodHrs = 1;

        services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(userTokenValidityPeriodHrs));

        services.AddSwaggerGen(c =>
        {
            c.DocumentFilter<AlphabeticEndpointOrderDocumentFilter>();
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = Constants.ApplicationName, Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddHttpClient(Constants.DefaultHttpClient)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
                };
                return handler;
            });
        services.AddMvcCore().AddControllersAsServices();
        return builder;
    }

    public static void ConfigureDefaults(this IApplicationBuilder app, IHostEnvironment hostEnvironment)
    {
        app.UseExceptionHandler();
        var servicesWrapper = app.ApplicationServices.GetRequiredService<ServicesWrapper>();
        app.InitializeEfCoreCaching(app.ApplicationServices.GetService<IConnectionMultiplexer>());
        app.ConfigureApplicationBuilderDefaults();
        app.UseRouting();
        app.ConfigureNLog();
        app.InitializeAuditing(servicesWrapper.AppSettings.AuditLogsTtlInDays);
        app.ConfigureSolhigsonFramework();
        app.UseMiddleware<ApiTraceMiddleware>();
        app.UseHttpsRedirection();
        app.UseHsts();
        if (hostEnvironment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                c => c.SwaggerEndpoint("v1/swagger.json", $"{Constants.ApplicationName} v1"));
        }

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[]
                { BasicAuthAuthorizationFilter.Default(servicesWrapper.AppSettings.HangfireDashboardAuth) },
            IgnoreAntiforgeryToken = true
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHangfireDashboard();
        });
        
        var machineName = string.Empty;
        try
        {
            machineName = Environment.MachineName;
        }
        catch
        {
            //
        }

        Logger.LogWarning("*** INITIALIZING APPLICATION *** [{machineName}]", machineName);
    }

    private static void ConfigureApplicationBuilderDefaults(this IApplicationBuilder app, bool forTests = false)
    {
        NewId.SetProcessIdProvider(new CurrentProcessIdProvider());

        TypeAdapterConfig.GlobalSettings.ForDestinationType<EntityBase>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Created)
            .Ignore(dest => dest.Updated);

        if (forTests)
        {
            return;
        }

        var serviceProvider = app.ApplicationServices.GetRequiredService<IServiceProvider>();
        Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
        {
            var claimsUser = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.User;
            scope.SetCustomField("User", claimsUser?.Email() ?? "System Initiated");
            scope.SetCustomField("Institution", claimsUser?.Institution() ?? "System Initiated");
            scope.SetCustomField("InstitutionId", claimsUser?.InstitutionId() ?? Constants.DefaultInstitutionId);
        });

        Audit.EntityFramework.Configuration.Setup()
            .ForContext<AppDbContext>(config => config
                .IncludeEntityObjects()
                .AuditEventType("EfCore"))
            .UseOptIn()
            .Include(typeof(SolhigsonPermission))
            .Include(typeof(SolhigsonRolePermission<string>));
    }

    private static void AddHangfireService(IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.HangfireConnectionString();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.Zero
            });
            config.UseConsole();
        });
        JobStorage.Current = new SqlServerStorage(connectionString);
    }

    private static IApplicationBuilder ConfigureNLog(this IApplicationBuilder app)
    {
        var lc = new LoggingConfiguration();
        lc.AddRule(LogLevel.Info, LogLevel.Fatal, GetDefaultFileTarget());
        lc.AddRule(LogLevel.Info, LogLevel.Fatal, GetDefaultConsoleTarget());
        NLog.LogManager.Configuration = lc;
        return app;
    }
    
    public static IApplicationBuilder ConfigureSolhigsonFramework(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.ConfigureSolhigsonServiceProviderWrapper();
        app.ConfigureSolhigsonLogManager();
        return app;
    }

    private static FileTarget GetDefaultFileTarget(bool isFallBack = false)
    {
        return new FileTarget
        {
            FileName = $"{Environment.CurrentDirectory}/log.log",
            Name = isFallBack ? "FileFallback" : "FileDefault",
            ArchiveAboveSize = 2560000,
            ArchiveNumbering = ArchiveNumberingMode.Sequence,
            Layout = "${date}|${uppercase:${level}}|${logger}${newline}${message} ${exception:format=tostring}${newline}"
        };
    }

    private static ColoredConsoleTarget GetDefaultConsoleTarget()
    {
        var target = new ColoredConsoleTarget();
        target.Name = "defaultConsoleTarget";
        //target.Layout = NLogDefaults.GetDefaultJsonLayout();
        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Error",
            ForegroundColor = ConsoleOutputColor.Black,
            BackgroundColor = ConsoleOutputColor.Red
        });
        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Warn",
            ForegroundColor = ConsoleOutputColor.Yellow,
            BackgroundColor = ConsoleOutputColor.NoChange
        });
        return target;
    }

    private static IApplicationBuilder InitializeAuditing(this IApplicationBuilder app,
        double expiryPeriodInDays)
    {
        var database = app.ApplicationServices.GetRequiredService<IMongoDatabase>();
        
        CreateMongoDbCollection<AuditLog>(database,
            TimeSpan.FromDays(expiryPeriodInDays), t => t.User,
            t => t.InstitutionId);
        
        var dataProvider = new MongoDataProvider(configurator =>
        {
            configurator.ClientSettings(database.Client.Settings);
            configurator.DatabaseSettings(database.Settings);
            configurator.Collection(MongoDbService.GetCollectionName<AuditLog>());
        });
        
        Configuration.DataProvider = dataProvider;

        return app;
    }

    private static IApplicationBuilder CreateMongoDbCollections(this IApplicationBuilder app, ServicesWrapper servicesWrapper)
    {
        var database = app.ApplicationServices.GetRequiredService<IMongoDatabase>();

        return app;
    }

    private static IMongoCollection<T> CreateMongoDbCollection<T>(this IMongoDatabase database,
        TimeSpan? expiryPeriod = null, params Expression<Func<T, object>>[] indexes) where T : IMongoDocumentBase
    {
        var clusteredIndexOptions = new ClusteredIndexOptions<T>
        {
            Key = Builders<T>.IndexKeys.Ascending(r => r.Id),
            Unique = true
        };
        database.CreateCollection(MongoDbService.GetCollectionName<T>(), new CreateCollectionOptions<T>
        {
            ClusteredIndex = clusteredIndexOptions,
        });
        var collection = database.GetCollection<T>(MongoDbService.GetCollectionName<T>());
        if (!indexes.HasData())
        {
            return collection;
        }
        foreach (var index in indexes)
        {
            var indexModel = new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(index));
            collection.Indexes.CreateOne(indexModel);
        }

        if (expiryPeriod is null)
        {
            return collection;
        }
        
        var ttlIndex = Builders<T>.IndexKeys.Descending(t => t.Ttl);
        collection.Indexes.CreateOne(
            new CreateIndexModel<T>(ttlIndex, new CreateIndexOptions
                {
                    ExpireAfter = expiryPeriod ?? TimeSpan.FromSeconds(0),
                    Background = true,
                }
            ));

        return collection;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        var healthChecks = app.MapGroup("");
        
        

        // Adding health checks endpoints to applications in non-development environments has security implications.
        if (!app.Environment.IsDevelopment())
        {
            healthChecks
                .CacheOutput("HealthChecks")
                .WithRequestTimeout("HealthChecks");
        }

        // All health checks must pass for app to be considered ready to accept traffic after starting
        healthChecks.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        healthChecks.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = static r => r.Tags.Contains("live")
        });

        return app;
    }

    public static void ConfigureDefaults(ServicesWrapper servicesWrapper)
    {
        TypeAdapterConfig.GlobalSettings.ForDestinationType<EntityBase>()
            .Ignore(dest => dest.Id);
        Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
        {
            scope.SetCustomField("User", servicesWrapper.ClaimsUser?.Email() ?? "System");
            scope.SetCustomField("Institution", servicesWrapper.ClaimsUser?.Institution() ?? "System");
            scope.SetCustomField("InstitutionId", servicesWrapper.ClaimsUser?.InstitutionId() ?? "System");
        });
    }
    
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
    
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        await dbContext.Database.MigrateAsync();
    }


}
