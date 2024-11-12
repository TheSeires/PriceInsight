using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.MarketCollectionName)]
public class Market : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string WebsiteUrl { get; set; } = string.Empty;
    
    public string IconUrl { get; set; } = string.Empty;
}
