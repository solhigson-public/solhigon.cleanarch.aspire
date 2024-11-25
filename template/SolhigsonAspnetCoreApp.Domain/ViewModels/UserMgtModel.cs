using Microsoft.AspNetCore.Mvc.Rendering;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record UserMgtModel
{
    private List<SelectListItem> _roles;

    private List<UserModel> _users;

    public List<UserModel> Users => _users ??= new List<UserModel>();
    public List<SelectListItem> SearchRoles => _roles ??= new List<SelectListItem>();
}