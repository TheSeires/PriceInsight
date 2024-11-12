using API.Models.Database;
using API.Models.Dto;

namespace API.Models.Mappers;

public static class ProductMapper
{
    public static Product MapToDomain(this ProductDto productDto)
    {
        return new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Description = productDto.Description,
            ImageUrl = productDto.ImageUrl,
            SourceMarketId = productDto.SourceMarketId,
            SourceCategory = productDto.SourceCategory,
            CategoryId = productDto.CategoryId,
            Aliases = productDto.Aliases,
            Attributes = productDto.Attributes,
            Added = productDto.Added,
            Updated = productDto.Updated,
        };
    }
}