namespace API.Models.Application;

public class PriceData
{
    public required string MarketId { get; init; }
    public required string ProductUrl { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountedPrice { get; init; }
}
