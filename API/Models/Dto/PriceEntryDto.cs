using API.Models.Database;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models.Dto;

public class PriceEntryDto : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = ObjectId.Empty.ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarketId { get; set; } = ObjectId.Empty.ToString();

    public Market? Market { get; set; }

    public string ProductUrl { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? DiscountedPrice { get; set; }

    public DateTime LastUpdated { get; set; }
}