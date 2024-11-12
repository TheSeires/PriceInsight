using MongoDB.Bson;

namespace API.Models.Application;

public class ProductToMatch
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required string ImageUrl { get; init; }
    public required Dictionary<string, string> Attributes { get; init; } = [];
}