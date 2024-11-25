using Newtonsoft.Json;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record UserModel
{
    private string _role = "<span style=\"color: red;\">Not Assigned</span>";
    public string Id { get; set; }

    public string UserName { get; set; }

    public string Lastname { get; set; }

    public string Firstname { get; set; }

    public string OtherNames { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Password { get; set; }

    public string RetypePassword { get; set; }

    public string InstitutionId { get; set; }

    public string Institution { get; set; }

    [JsonIgnore]
    public string RoleHtml
    {
        get => _role;
        set => _role = $"<span style=\"color: green;\">{value}</span>";
    }

    public string Role { get; set; }

    [JsonIgnore] public long UserUserRoleId { get; set; }

    [JsonIgnore] public string FullName => $"{Firstname} {OtherNames} {Lastname}";

    public bool Enabled { get; set; }

    public string StatusColor { get; set; }

    public string ActionTextDescription { get; set; }

    public string ActionText { get; set; }

    public string ActionId { get; set; }

    public string Icon { get; set; }

    public string IconColor { get; set; }

    public string PermissionKey { get; set; }

    public string Status { get; set; }

    public void CheckStatus()
    {
        if (!Enabled)
        {
            StatusColor = Constants.StatusBadge.DisabledColor;
            IconColor = "green-text";
            Icon = "check";
            Status = Constants.StatusBadge.DisabledText;
            ActionTextDescription = "Enable Account";
            ActionText = "enable";
            ActionId = Constants.Action.UserAction.EnableUser;
            PermissionKey = Permission.ConfigureUser;
        }
        else
        {
            StatusColor = Constants.StatusBadge.EnabledColor;
            IconColor = "red-text";
            Icon = "clear";
            Status = Constants.StatusBadge.EnabledText;
            ActionTextDescription = "Disable Account";
            ActionText = "disable";
            ActionId = Constants.Action.UserAction.DisableUser;
            PermissionKey = Permission.ConfigureUser;
        }
    }
}