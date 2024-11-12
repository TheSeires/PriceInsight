namespace API.Models.Dto.Requests;

public class UpdateCategoryRequest : CreateCategoryRequest
{
    public string Id { get; set; } = string.Empty;
}