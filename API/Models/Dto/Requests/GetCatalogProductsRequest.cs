namespace API.Models.Dto.Requests;

public class GetCatalogProductsRequest
{
    public string? Search { get; init; }
    public string? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool Discounted { get; init; } = false;
}