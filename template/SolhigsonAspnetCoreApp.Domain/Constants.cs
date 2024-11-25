using SolhigsonAspnetCoreApp.Domain.CacheModels;

namespace SolhigsonAspnetCoreApp.Domain;

public static class Constants
{
    public const string ApplicationName = "SolhigsonAspnetCoreApp";
    public const string JwtCookieName = "AccessToken";
    public const string DefaultDateTimeFormat = "MMM dd, yyyy HH:mm";
    public const string DefaultDateFormat = "MMM dd, yyyy";
    public const string DefaultInstitutionName = "SolhigsonAspnetCoreApp";
    public const string DefaultInstitutionId = "default0.0000.0000.0000.institution0";
    public const string DefaultHttpClient = "DefaultHttpClient";

    public static readonly InstitutionCacheModel SystemInstitution = new()
    {
        Id = DefaultInstitutionId,
        Name = DefaultInstitutionName
    };

    public static class ErrorCodes
    {
    }

    public static class Reports
    {
    }

    public static class Action
    {
        public static class UserAction
        {
            public const string DisableUser = "1";
            public const string EnableUser = "2";
        }
    }

    public static class AppRoles
    {
        public const string SystemAdministrator = "System Administrator";
        public const string InstitutionAdministrator = "Institution Administrator";

        public static class Groups
        {
            public const string System = "SYSTEM";
            public const string Institution = "INSTITUTION";
        }
    }

    public static class StatusBadge
    {
        public const string SuccessfulText = "Successful";
        public const string FailedText = "Failed";

        public const string EnabledText = "Enabled";
        public const string ApprovedText = "Approved";
        public const string DisabledText = "Disabled";
        public const string PendingText = "Pending";
        public const string DeclinedText = "Declined";
        public const string EnabledColor = "green lighten-5 green-text text-accent-4";
        public const string DisabledColor = "pink lighten-5 pink-text text-accent-2";
        public const string ApprovedColor = "green lighten-5 green-text text-accent-4";
        public const string PendingColor = "orange lighten-5 orange-text text-accent-4";
        public const string DeclinedColor = "red lighten-5 red-text text-accent-2";
        public const string NotApplicableColor = "grey lighten-3 grey-text text-accent-2";
        
        public const string EnabledTextColor = "green-text text-accent-4";
        public const string DisabledTextColor = "pink-text text-accent-2";
        public const string ApprovedTextColor = "green-text text-accent-4";
        public const string PendingTextColor = "orange-text text-accent-2";
        public const string DeclinedTextColor = "red-text text-accent-2";
        public const string NotApplicableTextColor = "grey-text text-accent-2";


        public static string ApprovedStatusText(bool isApproved)
        {
            return isApproved ? ApprovedText : PendingText;
        }

        public static string ApprovedStatusColor(bool isApproved)
        {
            return isApproved ? ApprovedColor : PendingColor;
        }

        public static string StatusText(bool isEnabled, string enabledText = EnabledText, string disabledText = DisabledText)
        {
            return isEnabled ? enabledText : disabledText;
        }

        public static string StatusColor(bool isEnabled)
        {
            return isEnabled ? EnabledColor : DisabledColor;
        }
    }

    public static class ClaimType
    {
        public const string InstitutionId = "InstitutionId";
        public const string Institution = "InstitutionName";
        public const string IsSystemUser = "IsSystemRole";
        public const string IsInstitutionUser = "IsInstitutionUser";
    }
}