namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record PermissionModel : ModelBase
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public bool IsMenu { get; set; }

    public bool IsMenuRoot { get; set; }

    public string ParentId { get; set; }

    public int MenuIndex { get; set; }

    public string Icon { get; set; }

    public string OnClickFunction { get; set; }

    public string AllowedRoles { get; set; }

    public string Parent { get; set; }
}