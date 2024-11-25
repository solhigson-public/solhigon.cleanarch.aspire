using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Solhigson.Framework.Infrastructure;
using Solhigson.Framework.Infrastructure.Dependency;

namespace SolhigsonAspnetCoreApp.Infrastructure.ApplicationSettings;

public class AppSettings(ConfigurationWrapper configurationWrapper, IWebHostEnvironment webHostEnvironment)
    : IDependencyInject
{
    public IConfiguration Configuration { get; } = configurationWrapper.Configuration;


    public string SandboxMarker(bool asHtml = true)
    {
        string text;
        if (webHostEnvironment.IsEnvironment("Local"))
            text = "LOCAL";
        else if (webHostEnvironment.IsDevelopment())
            text = "DEV";
        else if (webHostEnvironment.IsStaging())
            text = "UAT";
        else
            text = string.Empty;

        if (string.IsNullOrWhiteSpace(text)) return text;

        return asHtml ? $"<span style=\"font-style: italic; font-size: 18px;\">({text})</span>" : text;
    }


    #region Initialization Config

    private T GetInitializationConfig<T>(string config, string? defaultValue = null)
    {
        return configurationWrapper.GetConfigAsync<T>("Initialization", config, defaultValue).Result;
    }

    public string HangfireDashboardAuth => GetInitializationConfig<string>("HangfireDashboardAuth", "password");
    public string LogLevel => GetInitializationConfig<string>("LogLevel", "info");
    public bool LogApiTrace => GetInitializationConfig<bool>("LogApiTrace", "true");

    private string _protectedFields = string.Empty;
    private readonly List<string> _protectedFieldsList = new();

    public List<string> ProtectedFieldsList
    {
        get
        {
            var fields = ProtectedFields;
            if (string.Compare(fields, _protectedFields, StringComparison.OrdinalIgnoreCase) == 0)
                return _protectedFieldsList;

            _protectedFields = fields;
            _protectedFieldsList.Clear();
            _protectedFieldsList.AddRange(_protectedFields.Split(new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries));
            return _protectedFieldsList;
        }
    }

    public string ProtectedFields =>
        GetInitializationConfig<string>("ProtectedFields", "password,pin,authorization,key,token");

    public double LogsTtlInMinutes => GetInitializationConfig<double>("LogsTtlInMinutes", "5");
    public double AuditLogsTtlInDays => GetInitializationConfig<double>("AuditLogsTtlInDays", "90");
    public bool LogOutBoundApiTrace => GetInitializationConfig<bool>("LogOutBoundApiTrace", "true");
    public bool LogInBoundApiTrace => GetInitializationConfig<bool>("LogInBoundApiTrace", "true");

    #endregion

    #region Jwt

    private T GetJwtConfig<T>(string config, string? defaultValue = null)
    {
        return configurationWrapper.GetConfigAsync<T>("Jwt", config, defaultValue).Result;
    }

    public string JwtKey => GetJwtConfig<string>("Key");
    public double JwtExpirationHrs => GetJwtConfig<double>("ExpirationHrs", "2");

    #endregion

    #region Cosmos Db

    private T GetCosmosDbConfig<T>(string config, string? defaultValue = null)
    {
        return configurationWrapper.GetConfigAsync<T>("CosmosDb", config, defaultValue).Result;
    }

    public string CosmosDbConnString => GetCosmosDbConfig<string>("ConnString", "");

    public string CosmosDbDatabaseName => GetCosmosDbConfig<string>("Database", "");
    public string CosmosDbLogsContainer => GetCosmosDbConfig<string>("LogsContainer", "");
    public string CosmosDbAuditLogsContainer => GetCosmosDbConfig<string>("AuditLogsContainer");

    #endregion

    #region General App Settings

    private T GetAppSettingConfig<T>(string config, string? defaultValue = null, bool getFromAppSettingFileOnly = false)
    {
        if (getFromAppSettingFileOnly)
            return configurationWrapper.GetFromAppSettingFileOnlyAsync<T>("appSettings", config, defaultValue).Result;
        return configurationWrapper.GetConfigAsync<T>("appSettings", config, defaultValue).Result;
    }

    public int UserTokenValidityPeriodHrs =>
        GetAppSettingConfig<int>("UserTokenValidityPeriodHrs", "2", true);


    public string DefaultFromEmailAddress =>
        GetAppSettingConfig<string>("DefaultFromEmailAddress", "email@example.com");

    public string DefaultFromEmailDisplayName =>
        GetAppSettingConfig<string>("DefaultFromEmailDisplayName", "SolhigsonAspnetCoreApp");

    public string EmailTemplateHeader => GetAppSettingConfig<string>("EmailTemplateHeader", "");
    public string StylesAndScriptsVersion => GetAppSettingConfig<string>("StylesAndScriptsVersion", "001");

    public string LogJavascriptConsoleLogs => GetAppSettingConfig<string>("LogJavascriptConsoleLogs", "true");

    #endregion

    #region Smtp

    private T GetSmtpConfig<T>(string config, string? defaultValue = null)
    {
        return configurationWrapper.GetConfigAsync<T>("Smtp", config, defaultValue).Result;
    }

    public string SmtpServer => GetSmtpConfig<string>("Server", "smtp.office365.com");
    public int SmtpPort => GetSmtpConfig<int>("Port", "587");
    public string SmtpUsername => GetSmtpConfig<string>("Username", "support@safi.africa");
    public string SmtpPassword => GetSmtpConfig<string>("Password", "xxx");
    public bool SmtpEnableSsl => GetSmtpConfig<bool>("EnableSsl", "true");

    #endregion
}