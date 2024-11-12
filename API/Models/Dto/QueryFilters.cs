using MongoDB.Driver;

namespace API.Models.Dto;

public class QueryFilters
{
    public int? Page { get; init; }
    public int Limit { get; init; }
    public string? SortBy { get; private init; }
    public SortDirection SortDirection { get; private init; }
    public Dictionary<string, List<string>>? MatchFilters { get; private init; }
    
    public static ValueTask<QueryFilters> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;
        Dictionary<string, List<string>>? matchFilters = null;

        foreach (var (key, value) in query)
        {
            if (key.StartsWith("match") == false)
            {
                continue;
            }

            var startIndex = key.IndexOf('[') + 1;
            var endIndex = key.IndexOf(']', startIndex);

            if (startIndex <= 0 || endIndex <= startIndex)
            {
                continue;
            }

            var property = key[startIndex..endIndex];
            matchFilters ??= [];
            if (matchFilters.TryGetValue(property, out var values))
            {
                values.AddRange(value);
            }
            else
            {
                var valueList = new List<string>(value);
                matchFilters[property] = valueList;
            }
        }

        var pageWasParsed = int.TryParse(query["page"], out var page);
        _ = int.TryParse(query["limit"], out var limit);
        string? sortBy = query["sortBy"];
        string? sortDirStr = query["sortDirection"];
        _ = Enum.TryParse<SortDirection>(sortDirStr, true, out var sortDirection);

        var result = new QueryFilters
        {
            Page = pageWasParsed ? page : null,
            Limit = limit,
            SortBy = sortBy,
            SortDirection = sortDirection,
            MatchFilters = matchFilters
        };

        return ValueTask.FromResult(result);
    }
}