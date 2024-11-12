namespace API.Models.Application;

public class QueryFiltersResult
{
    public bool MatchFiltersApplied { get; init; }
    public bool SortFilterApplied { get; init; }
    public bool PaginationFilterApplied { get; init; }
    public int NormalizedLimit { get; init; }

    public static readonly QueryFiltersResult Empty = new(false, false, false);

    public QueryFiltersResult()
    {
    }

    private QueryFiltersResult(bool matchFiltersApplied, bool sortFilterApplied, bool paginationFilterApplied)
    {
        MatchFiltersApplied = matchFiltersApplied;
        SortFilterApplied = sortFilterApplied;
        PaginationFilterApplied = paginationFilterApplied;
    }
}