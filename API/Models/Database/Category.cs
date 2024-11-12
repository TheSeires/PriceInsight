using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.CategoryCollectionName)]
public class Category : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ParentId { get; set; } = ObjectId.Empty.ToString();

    public string NameTranslationKey { get; set; } = string.Empty;

    public string ShortId { get; set; } = string.Empty;
}
