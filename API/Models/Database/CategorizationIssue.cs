using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.CategorizationIssueCollectionName)]
public class CategorizationIssue : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    public string SourceCategory { get; set; } = string.Empty;

    public string SourceProductUrl { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarketId { get; set; } = ObjectId.Empty.ToString();
}
