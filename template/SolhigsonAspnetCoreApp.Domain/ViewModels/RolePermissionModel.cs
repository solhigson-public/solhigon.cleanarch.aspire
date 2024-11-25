using Microsoft.AspNetCore.Mvc.Rendering;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record RolePermissionModel
{
    public string Role { get; set; }
    public string RoleId { get; set; }

    public List<RolePermission> Permissions { get; set; }
}

public class RolePermission
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SelectListItem> Children { get; set; }
}