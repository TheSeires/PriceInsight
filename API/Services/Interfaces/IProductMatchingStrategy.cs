using API.Models.Application;
using API.Models.Dto;

namespace API.Services.Interfaces;

public interface IProductMatchingStrategy
{
    (ProductDto Product, RequiredDatabaseAction Action) MatchOrCreateProduct(ProductToMatch productToMatch,
        string marketId, string? mappedCategoryId, List<ProductDto> allProductsIncludingPrices, double matchThreshold);
}