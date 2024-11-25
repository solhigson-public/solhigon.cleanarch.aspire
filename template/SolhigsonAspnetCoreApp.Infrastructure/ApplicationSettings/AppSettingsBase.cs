using Microsoft.AspNetCore.Hosting;
using Solhigson.Framework.Infrastructure;
using Solhigson.Framework.Infrastructure.Dependency;

namespace SolhigsonAspnetCoreApp.Infrastructure.ApplicationSettings;

public abstract class AppSettingsBase : IDependencyInject
{
    private readonly string _groupName;
    private readonly ConfigurationWrapper _configurationWrapper;
    protected readonly IWebHostEnvironment WebHostEnvironment;

    protected AppSettingsBase(string groupName, ConfigurationWrapper configurationWrapper, IWebHostEnvironment webHostEnvironment)
    {
        _groupName = groupName;
        _configurationWrapper = configurationWrapper;
        WebHostEnvironment = webHostEnvironment;
    }
    
    protected T GetConfiguration<T>(string config, string defaultValue = null, bool useAppSettingOnly = false)
    {
        return _configurationWrapper.GetConfigAsync<T>(_groupName, config, defaultValue, useAppSettingOnly).Result;
    }
    
    protected string GetConfiguration(string config, string defaultValue = null, bool useAppSettingOnly = false)
    {
        return _configurationWrapper.GetConfigAsync<string>(_groupName, config, defaultValue, useAppSettingOnly).Result;
    }

}