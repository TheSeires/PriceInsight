using API.Models.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models.Dto;

public class CategorizationIssueDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    public string SourceCategory { get; set; } = string.Empty;

    public string SourceProductUrl { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarketId { get; set; } = ObjectId.Empty.ToString();

    public Market? Market { get; set; }

    public List<Market>? Markets { get; set; }
}
