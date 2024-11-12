namespace API.Models.Application;

public class PaginatedItemsResponse<T>
{
    public List<T> Items { get; init; }
    public long TotalItems { get; init; }
    public long Limit { get; init; }

    public PaginatedItemsResponse(List<T> items, long totalItems, long limit)
    {
        Items = items;
        TotalItems = totalItems;
        Limit = limit;
    }
}
