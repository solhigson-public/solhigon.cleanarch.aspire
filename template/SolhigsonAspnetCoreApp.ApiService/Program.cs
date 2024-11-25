using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using NLog.Web;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Logging;
using SolhigsonAspnetCoreApp.Application;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

configuration.AddJsonFile("appsettings.json")
    .AddJsonFile("secrets/appsettings.secrets.json", optional: true)
    .AddJsonFile($"secrets/appsettings.{env.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();



builder.ConfigureCommonServices(Assembly.GetExecutingAssembly());
// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.ConfigureDefaults(env);
app.MapDefaultEndpoints();

app.Run();