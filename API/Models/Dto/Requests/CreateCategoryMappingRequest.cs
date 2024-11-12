using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace API.Models.Application.Dto.Requests;

public class CreateCategoryMappingRequest
{
    public string SourceMarketId { get; init; } = ObjectId.Empty.ToString();

    [MinLength(4)]
    public string SourceCategory { get; init; } = string.Empty;
    public string TargetCategoryId { get; init; } = string.Empty;
}
