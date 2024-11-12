using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.PriceChangeCollectionName)]
public class PriceChange : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = ObjectId.Empty.ToString();

    [BsonIgnore]
    public Product? Product { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarketId { get; set; } = ObjectId.Empty.ToString();

    [BsonIgnore]
    public Market? Market { get; set; }

    public decimal Price { get; set; }

    public DateTime Changed { get; set; }
}
