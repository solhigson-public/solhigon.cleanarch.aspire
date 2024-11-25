using Newtonsoft.Json;
using Solhigson.Framework.Identity;
using SolhigsonAspnetCoreApp.Domain.CacheModels;

namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record SessionUser
{
    private string _auditName;


    private string? _displayName;
    public string Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }

    public bool IsLockedOut { get; set; }
    public bool RequiresTwoFactor { get; set; }

    public string InstitutionId { get; set; }

    public InstitutionCacheModel? Institution { get; set; }

    public bool RequirePasswordChange { get; set; }
    public string UserName { get; set; }

    public SolhigsonAspNetRole? Role { get; set; }

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string DisplayName => _displayName ??=
        $"{Firstname} {Lastname} [{Role.Name}] - {Institution?.Name ?? Constants.DefaultInstitutionName}";

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string AuditName =>
        _auditName ??= $"{Firstname} {Lastname} [{Email}]";

    public string FilteredInstitutionId(string value)
    {
        return InstitutionId == Constants.DefaultInstitutionId ? value : InstitutionId;
    }
}