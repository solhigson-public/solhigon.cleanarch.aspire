namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record RoleModel : ModelBase
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string RoleGroupId { get; set; }

    public string RoleGroup { get; set; }

    public string StartPage { get; set; }
}