using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace API.Models.Dto.Requests;

public class CreateCategoryRequest
{
    [MaxLength(64, ErrorMessage = "models.category.validation.nameIsTooLong")]
    public required string Name { get; init; } = string.Empty;
    public string ParentId { get; init; } = ObjectId.Empty.ToString();
    public string NameTranslationKey { get; init; } = string.Empty;
    [Required(ErrorMessage = "models.category.validation.shortIdRequired")]
    public string ShortId { get; init; } = string.Empty;
}
