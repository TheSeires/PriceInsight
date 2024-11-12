using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.ProductCollectionName)]
public class Product : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.Empty.ToString();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string SourceMarketId { get; init; } = ObjectId.Empty.ToString();

    public string SourceCategory { get; init; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = ObjectId.Empty.ToString();

    public List<ProductAlias> Aliases { get; set; } = [];

    public Dictionary<string, string> Attributes { get; set; } = [];

    public DateTime Added { get; set; } = DateTime.UtcNow;

    public DateTime Updated { get; set; } = DateTime.UtcNow;
}