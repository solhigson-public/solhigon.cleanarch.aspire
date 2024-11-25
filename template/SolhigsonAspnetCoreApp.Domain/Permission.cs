using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Domain;

public static class Permission
{
    public const string ConfigureUser = "permission.users.configure";
    public const string ViewUsers = "permission.users.view";
    public const string ViewApplicationLogs = "application.logs.view";
    public const string ViewApiLogs = "api.logs.view";
    public const string ViewAuditLogs = "audit.logs.view";
    public const string ConfigureRolePermissions = "permissions.roles.configure";
    public const string ConfigurePermissions = "permissions.configure";
    public const string ConfigureCountries = "countries.configure";
    public const string ConfigureInstitutions = "institutions.configure";
    public const string ConfigureCurrency = "currency.configure";
    public const string ConfigureAppSettings = "appsettings.configure";
    public const string ConfigureNotificationTemplates = "notificationtemplates.configure";
    public const string Roles = "permissions.roles";


    public static Dictionary<string, string> Build()
    {
        var fields = typeof(Permission).GetFields();
        var permissions = new Dictionary<string, string>();
        foreach (var field in fields)
            permissions.TryAdd($"{field.GetRawConstantValue()}",
                HelperFunctions.SeparatePascalCaseWords(field.Name));
        return permissions;
    }
}