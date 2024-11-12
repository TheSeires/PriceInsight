using API.Models.Application.Dto.Requests;
using API.Models.Database;

namespace API.Models.Mappers;

public static class CategoryMapper
{
    public static CategoryMapping MapToCategoryMapping(
        this CreateCategoryMappingRequest createCategoryMapping
    )
    {
        return new CategoryMapping
        {
            SourceCategory = createCategoryMapping.SourceCategory,
            SourceMarketId = createCategoryMapping.SourceMarketId,
            TargetCategoryId = createCategoryMapping.TargetCategoryId,
        };
    }
}
