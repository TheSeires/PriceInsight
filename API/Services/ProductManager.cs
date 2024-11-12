using API.Extensions;
using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;

namespace API.Services;

public class ProductManager : IProductManager
{
    private readonly IProductService _productService;
    private readonly ICachedPriceEntryService _cachedPriceEntryService;
    private readonly ILogger<ProductManager> _logger;

    public ProductManager(
        IProductService productService,
        ICachedPriceEntryService cachedPriceEntryService,
        ILogger<ProductManager> logger
    )
    {
        _productService = productService;
        _cachedPriceEntryService = cachedPriceEntryService;
        _logger = logger;
    }

    public async Task UpsertProductsDataAsync(
        List<ProductMatchingResult> productMatchingResults,
        CancellationToken cancellationToken)
    {
        var productsToCreate = new List<Product>();
        var productsToUpdate = new List<Product>();
        var productsToDelete = new List<Product>();
        var priceEntriesToCreate = new List<PriceEntry>();
        var priceEntriesToUpdate = new List<PriceEntry>();
        var priceEntriesToDelete = new List<PriceEntry>();
        //var priceChangesToCreate = new List<PriceChange>();

        foreach (var result in productMatchingResults)
        {
            if (result.ProductAction == RequiredDatabaseAction.Create)
            {
                productsToCreate.Add(result.ProductDto);
            }
            else if (result.ProductAction == RequiredDatabaseAction.Update)
            {
                productsToUpdate.Add(result.ProductDto);
            }
            else if (result.ProductAction == RequiredDatabaseAction.Delete)
            {
                productsToDelete.Add(result.ProductDto);
            }

            if (result.PriceEntryAction == RequiredDatabaseAction.Create)
            {
                priceEntriesToCreate.Add(result.PriceEntry);
            }
            else if (result.PriceEntryAction == RequiredDatabaseAction.Update)
            {
                priceEntriesToUpdate.Add(result.PriceEntry);
            }
            else if (result.PriceEntryAction == RequiredDatabaseAction.Delete)
            {
                priceEntriesToDelete.Add(result.PriceEntry);
            }

            //priceChangesToCreate.Add(new PriceChange { Changed = result.PriceEntry.LastUpdated });
        }

        _logger.LogInformation("Products: create - '{create}', update - '{update}', delete - '{delete}'",
            productsToCreate.Count, productsToUpdate.Count, productsToDelete.Count);
        _logger.LogInformation("Price entries: create - '{create}', update - '{update}', delete - '{delete}'",
            priceEntriesToCreate.Count, priceEntriesToUpdate.Count, priceEntriesToDelete.Count);

        await _productService.CreateManyAsync(productsToCreate, cancellationToken).AsSafeExecutionAsync(_logger);
        await _productService.BulkUpdateAsync(productsToUpdate, cancellationToken: cancellationToken)
            .AsSafeExecutionAsync(_logger);
        await _productService.DeleteManyAsync(productsToDelete, cancellationToken).AsSafeExecutionAsync(_logger);

        await _cachedPriceEntryService.CreateManyAsync(priceEntriesToCreate, cancellationToken)
            .AsSafeExecutionAsync(_logger);
        await _cachedPriceEntryService.BulkUpdateAsync(priceEntriesToUpdate, cancellationToken: cancellationToken)
            .AsSafeExecutionAsync(_logger);
        await _cachedPriceEntryService.DeleteManyAsync(priceEntriesToDelete, cancellationToken)
            .AsSafeExecutionAsync(_logger);
    }
}