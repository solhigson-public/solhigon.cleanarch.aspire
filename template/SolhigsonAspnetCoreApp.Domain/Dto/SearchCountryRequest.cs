namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record SearchCountryRequest : PagedRequestBase
{
    public bool IncludeNonActive { get; set; }
}