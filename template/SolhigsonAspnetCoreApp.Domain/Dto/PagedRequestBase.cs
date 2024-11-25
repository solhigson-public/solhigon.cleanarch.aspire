namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record PagedRequestBase
{
    private int _itemsPerPage;
    private int _page;

    public int PageNumber
    {
        get
        {
            if (_page < 1) _page = 1;

            return _page;
        }
        set => _page = value;
    }

    public int ItemsPerPage
    {
        get
        {
            if (_itemsPerPage < 10) _itemsPerPage = 10;
            return _itemsPerPage;
        }
        set => _itemsPerPage = value;
    }

    public string InstitutionId { get; set; }
    public string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}