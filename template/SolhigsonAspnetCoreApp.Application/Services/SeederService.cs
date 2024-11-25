using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Persistence.EntityModels;
using Solhigson.Framework.Persistence.Repositories.Abstractions;
using Solhigson.Framework.Services;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Infrastructure;
using SolhigsonAspnetCoreApp.Infrastructure.ApplicationSettings;
using IRepositoryWrapper = SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IRepositoryWrapper;
using Roles = SolhigsonAspnetCoreApp.Domain.Constants.AppRoles;

namespace SolhigsonAspnetCoreApp.Application.Services;

public class SeederService : ServiceBase
{
    private const string UserManagementPermission = "user.management";
    private const string ConfigurationPermission = "system.configuration";
    private const string LogsPermission = "system.logs";
    private const string ReportsPermission = "system.reports";
    private readonly SolhigsonIdentityManager<AppUser, AppDbContext> _identityManager;
    private readonly INotificationTemplateRepository _notificationTemplateRepository;
    private readonly SolhigsonConfigurationService _solhigsonConfigurationService;

    public SeederService(IRepositoryWrapper repositoryWrapper, ServicesWrapper servicesWrapper,
        SolhigsonConfigurationService solhigsonConfigurationService,
        Solhigson.Framework.Persistence.Repositories.Abstractions.IRepositoryWrapper solhigsonRepositoryWrapper) : base(
        repositoryWrapper)
    {
        _identityManager = servicesWrapper.IdentityManager;
        _solhigsonConfigurationService = solhigsonConfigurationService;
        _notificationTemplateRepository = solhigsonRepositoryWrapper.NotificationTemplateRepository;
    }

    public async Task Seed(Assembly controllersAssembly)
    {
        var discoverInfo =
            await _identityManager.PermissionManager.DiscoverNewPermissionsAsync(controllersAssembly,
                Permission.Build());
        this.ELogDebug("Permissions DiscoveryDetails", discoverInfo);
        await AddAllNotificationTemplatesAsync();
        InitializeSettings();

        if (await _identityManager.RoleGroupManager.HasRoleGroups()) return;

        await ConfigurePermissionsAsync();
        await ConfigureRolesAsync();
        await ConfigureRoleAccessAsync();
        await AddUsersAsync();
        await AddCountriesAsync();
        await AddCurrenciesAsync();
    }

    private async Task AddUsersAsync()
    {
        var listOfUsers = new List<AppUser>
        {
            new()
            {
                Email = "admin@example.com",
                UserName = "admin@example.com",
                PhoneNumber = "08000000000",
                Firstname = "System",
                Lastname = "Admin",
                Enabled = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                InstitutionId = Constants.DefaultInstitutionId
            }
        };

        foreach (var user in listOfUsers)
        {
            var result = await _identityManager.UserManager.CreateAsync(user, "password");

            if (result.Succeeded) await _identityManager.UserManager.AddToRoleAsync(user, Roles.SystemAdministrator);
        }
    }

    private void InitializeSettings()
    {
        try
        {
            var props = typeof(AppSettings).GetProperties();
            foreach (var prop in props) prop.GetValue(ServicesWrapper.AppSettings);
        }
        catch (Exception e)
        {
            //this.ELogError(e);
        }
    }

    private async Task ConfigureRolesAsync()
    {
        await _identityManager.RoleGroupManager.CreateAsync(Roles.Groups.System);
        await _identityManager.RoleGroupManager.CreateAsync(Roles.Groups.Institution);

        await _identityManager.CreateRoleAsync(Roles.SystemAdministrator, Roles.Groups.System);

        await _identityManager.CreateRoleAsync(Roles.InstitutionAdministrator, Roles.Groups.Institution);
    }

    private async Task ConfigurePermissionsAsync()
    {
        await AddTopLevelPermission(UserManagementPermission, "User Management", "person_outline");
        await AddTopLevelPermission(ConfigurationPermission, "Configuration", "settings");
        await AddTopLevelPermission(LogsPermission, "Logs", "format_list_bulleted");
        await AddTopLevelPermission(ReportsPermission, "Reports", "view_list");

        //User Management
        await AddPermissionToParentAsync(Permission.Roles, UserManagementPermission);
        await AddPermissionToParentAsync(Permission.ConfigureRolePermissions, UserManagementPermission);
        await AddPermissionToParentAsync(Permission.ConfigureUser, UserManagementPermission);
        await AddPermissionToParentAsync(Permission.ViewUsers, UserManagementPermission);
        await AddPermissionToParentAsync(Permission.ConfigurePermissions, UserManagementPermission);

        //Configuration
        await AddPermissionToParentAsync(Permission.ConfigureAppSettings, ConfigurationPermission);
        await AddPermissionToParentAsync(Permission.ConfigureNotificationTemplates, ConfigurationPermission);
        await AddPermissionToParentAsync(Permission.ConfigureCountries, ConfigurationPermission);
        await AddPermissionToParentAsync(Permission.ConfigureCurrency, ConfigurationPermission);
        await AddPermissionToParentAsync(Permission.ConfigureInstitutions, ConfigurationPermission);

        //Logs
        await AddPermissionToParentAsync(Permission.ViewApplicationLogs, LogsPermission);
        await AddPermissionToParentAsync(Permission.ViewApiLogs, LogsPermission);
        await AddPermissionToParentAsync(Permission.ViewAuditLogs, LogsPermission);
    }

    private async Task AddPermissionToParentAsync(string permission, string parentPermission)
    {
        var perm = await RepositoryWrapper.DbContext.Permissions.FirstOrDefaultAsync(t => t.Name == permission);
        var parentPerm =
            await RepositoryWrapper.DbContext.Permissions.FirstOrDefaultAsync(t => t.Name == parentPermission);
        if (perm is null || parentPerm is null) return;

        perm.ParentId = parentPerm.Id;
        await RepositoryWrapper.DbContext.SaveChangesAsync();
    }

    private async Task ConfigureRoleAccessAsync()
    {
        await GiveAccessToPermissionAsync(Permission.ConfigureCountries, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ViewUsers, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureUser, Roles.SystemAdministrator);

        await GiveAccessToPermissionAsync(Permission.Roles, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureRolePermissions, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigurePermissions, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureAppSettings, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureNotificationTemplates, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ViewApplicationLogs, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ViewApiLogs, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ViewAuditLogs, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureCurrency, Roles.SystemAdministrator);
        await GiveAccessToPermissionAsync(Permission.ConfigureInstitutions, Roles.SystemAdministrator);
    }

    private async Task GiveAccessToPermissionAsync(string permission, params string[] roles)
    {
        foreach (var role in roles) await _identityManager.PermissionManager.GiveAccessToRoleAsync(role, permission);
    }

    private async Task AddCountriesAsync()
    {
        var country = RepositoryWrapper.CountryRepository.New();
        country.Code = "NG";
        country.Name = "Nigeria";
        country.Enabled = true;

        await RepositoryWrapper.SaveChangesAsync();
    }

    private async Task AddCurrenciesAsync()
    {
        var currency = RepositoryWrapper.CurrencyRepository.New();
        currency.Name = "Naira";
        currency.AlphabeticCode = "NGN";
        currency.NumericCode = "566";
        currency.Symbol = "₦";

        currency = RepositoryWrapper.CurrencyRepository.New();
        currency.Name = "United States Dollar";
        currency.AlphabeticCode = "USD";
        currency.NumericCode = "840";
        currency.Symbol = "$";

        currency = RepositoryWrapper.CurrencyRepository.New();
        currency.Name = "Pound Sterling";
        currency.AlphabeticCode = "GBR";
        currency.NumericCode = "826";
        currency.Symbol = "£";

        currency = RepositoryWrapper.CurrencyRepository.New();
        currency.Name = "Euro";
        currency.AlphabeticCode = "EUR";
        currency.NumericCode = "978";
        currency.Symbol = "€";

        await RepositoryWrapper.SaveChangesAsync();
    }

    private async Task AddTopLevelPermission(string name, string description, string icon)
    {
        var permission = new SolhigsonPermission
        {
            Name = name,
            Description = description,
            IsMenuRoot = true,
            IsMenu = true,
            Enabled = true,
            Icon = icon
        };
        var existing = await RepositoryWrapper.DbContext.Permissions.FirstOrDefaultAsync(t => t.Name == name);
        if (existing is not null)
        {
            existing.IsMenuRoot = permission.IsMenuRoot;
            existing.Icon = permission.Icon;
            await RepositoryWrapper.SaveChangesAsync();
            return;
        }

        await _identityManager.PermissionManager.AddPermissionAsync(permission);
        await RepositoryWrapper.SaveChangesAsync();
    }

    private async Task AddAllNotificationTemplatesAsync()
    {
        var templates = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "EmailTemplates"), "*.html",
            SearchOption.TopDirectoryOnly);
        foreach (var file in templates) await AddNotificationTemplateAsync(file);
    }

    private async Task AddNotificationTemplateAsync(string path)
    {
        await using var fileStream = File.OpenRead(path);
        using var streamReader = new StreamReader(fileStream);
        var template = await streamReader.ReadToEndAsync();

        var name = Path.GetFileName(path).Replace(".html", "");
        if (!await _notificationTemplateRepository.ExistsAsync(t => t.Name == name))
            await _solhigsonConfigurationService.SaveNotificationTemplateAsync(new NotificationTemplate
            {
                Name = Path.GetFileName(path).Replace(".html", ""),
                Template = template
            });
    }
}