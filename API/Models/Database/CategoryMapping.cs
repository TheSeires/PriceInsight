using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.CategoryMappingCollectionName)]
public class CategoryMapping : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string SourceMarketId { get; set; } = ObjectId.Empty.ToString();

    public string SourceCategory { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string TargetCategoryId { get; set; } = ObjectId.Empty.ToString();
}
