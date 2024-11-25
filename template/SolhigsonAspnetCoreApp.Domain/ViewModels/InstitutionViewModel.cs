using SolhigsonAspnetCoreApp.Domain.Dto;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record InstitutionViewModel
{
    private List<InstitutionModel>? _institutions;

    public List<InstitutionModel> Institutions => _institutions ??= [];
}

public record InstitutionModel : InstitutionDto
{
    public string PortalStatus => Constants.StatusBadge.StatusText(EnablePortalAccess);
    public string PortalStatusColor => Constants.StatusBadge.StatusColor(EnablePortalAccess);
}