using API.Models.Application;

namespace API.Services.Interfaces;

public interface IProductManager
{
    Task UpsertProductsDataAsync(
        List<ProductMatchingResult> productMatchingResults,
        CancellationToken cancellationToken
    );
}
