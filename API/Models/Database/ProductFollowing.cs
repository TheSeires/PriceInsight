using API.Models.Application;
using API.Models.Database.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.ProductFollowingCollectionName)]
public class ProductFollowing : IEntity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = default!;

    public required string UserId { get; set; }

    public required string ProductId { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
}