using API.Models.Application;

namespace API.Services.Interfaces;

public interface IProductMatcher
{
    Task<List<ProductMatchingResult>> MatchOrCreateProductsAsync(List<ProductParseResult> parsedResults,
        CancellationToken cancellationToken);
}