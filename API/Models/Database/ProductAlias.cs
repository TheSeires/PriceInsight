namespace API.Models.Database;

public class ProductAlias
{
    public string Name { get; set; }
    public string MarketId { get; set; }

    public ProductAlias(string name, string marketId)
    {
        Name = name;
        MarketId = marketId;
    }
}