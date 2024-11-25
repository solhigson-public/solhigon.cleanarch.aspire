using SolhigsonAspnetCoreApp.Domain.Dto;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record CountryModel : CountryDto
{
    public string Status => Constants.StatusBadge.StatusText(Enabled);
    public string StatusColor => Constants.StatusBadge.StatusColor(Enabled);
}