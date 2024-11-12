using API.Extensions;
using API.Models.Application;
using API.Models.Database;
using API.Models.Dto;
using API.Models.Mappers;
using API.Services.Interfaces;

namespace API.Services;

public class ProductMatcher : IProductMatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductMatcher> _logger;
    private readonly IProductMatchingStrategy _productMatchingStrategy;

    private const double MatchThreshold = 80; // 0 - 100
    private const int ProgressLogCount = 20;

    public ProductMatcher(
        IServiceProvider serviceProvider,
        ILogger<ProductMatcher> logger,
        IProductMatchingStrategy productMatchingStrategy)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _productMatchingStrategy = productMatchingStrategy;
    }

    public async Task<List<ProductMatchingResult>> MatchOrCreateProductsAsync(List<ProductParseResult> parsedProducts,
        CancellationToken cancellationToken)
    {
        var results = new List<ProductMatchingResult>();

        using var scope = _serviceProvider.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
        var cachedPriceEntryService = scope.ServiceProvider.GetRequiredService<ICachedPriceEntryService>();

        var allProductsIncludingPrices = await productService.FindByAsync(
            new AggregateOperator<Product>().Lookup<PriceEntry, ProductDto>(
                DbCollectionNames.PriceEntryCollectionName,
                prod => prod.Id,
                price => price.ProductId,
                prod => prod.PriceEntries),
            cancellationToken);

        AddProductsToDeleteToResults(results, ref allProductsIncludingPrices, parsedProducts);

        var cachedCategoryMappingService = scope.ServiceProvider.GetRequiredService<ICachedCategoryMappingService>();
        var cachedCategorizationIssueService =
            scope.ServiceProvider.GetRequiredService<ICachedCategorizationIssueService>();

        var parsedProductsCount = parsedProducts.Count;
        var progressLogInterval = Math.Max(1, parsedProductsCount / ProgressLogCount);

        foreach (var (index, parsedProduct) in parsedProducts.Index())
        {
            try
            {
                if (index % progressLogInterval == 0 || index == parsedProducts.Count - 1)
                {
                    var progress = (index + 1) * 100 / parsedProductsCount;
                    _logger.LogInformation("Product matching progress: {progress}%", progress);
                }

                var matchingResult = await ProcessParsedProductAsync(parsedProduct, allProductsIncludingPrices,
                    cachedCategoryMappingService, cachedCategorizationIssueService, cachedPriceEntryService,
                    cancellationToken);

                results.Add(matchingResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing product '{ProductName}'",
                    parsedProduct.ProductToMatch.Name);
            }
        }

        return results;
    }

    private void AddProductsToDeleteToResults(List<ProductMatchingResult> results,
        ref List<ProductDto> allProductsIncludingPrices, List<ProductParseResult> parsedProducts)
    {
        var marketIds = parsedProducts.Select(p => p.PriceData.MarketId).Distinct().ToHashSet();
        var parsedProductUrls = parsedProducts.Select(p => p.PriceData.ProductUrl).ToHashSet();

        var productsToDelete = allProductsIncludingPrices.Where(p =>
            p.PriceEntries?.Count == 1 && marketIds.Contains(p.PriceEntries[0].MarketId) &&
            parsedProductUrls.Contains(p.PriceEntries[0].ProductUrl) == false).ToList();

        if (productsToDelete.Count == 0)
            return;

        results.AddRange(productsToDelete.Select(p =>
            new ProductMatchingResult(p.MapToDomain(), p.PriceEntries![0].MapToDomain(), RequiredDatabaseAction.Delete,
                RequiredDatabaseAction.Delete)));

        allProductsIncludingPrices = allProductsIncludingPrices.Except(productsToDelete).ToList();
    }

    private async Task<ProductMatchingResult> ProcessParsedProductAsync(
        ProductParseResult parsedProduct,
        List<ProductDto> allProductsIncludingPrices,
        ICachedCategoryMappingService cachedCategoryMappingService,
        ICachedCategorizationIssueService cachedCategorizationIssueService,
        ICachedPriceEntryService cachedPriceEntryService,
        CancellationToken cancellationToken)
    {
        var productToMatch = parsedProduct.ProductToMatch;
        var priceData = parsedProduct.PriceData;

        var mappedCategoryId =
            await cachedCategoryMappingService.GetTargetCategoryIdAsync(productToMatch.Category, priceData.MarketId,
                cancellationToken);

        var (productIncludingPrices, productAction) = _productMatchingStrategy.MatchOrCreateProduct(productToMatch,
            priceData.MarketId, mappedCategoryId, allProductsIncludingPrices, MatchThreshold);

        var (priceEntry, priceEntryAction) = await FindOrCreateProductPriceEntryAsync(productIncludingPrices,
            productAction, priceData, cachedPriceEntryService);

        if (mappedCategoryId.IsNullOrWhiteSpace())
        {
            await cachedCategorizationIssueService.CreateIfNotExistsAsync(productToMatch.Category, priceData.MarketId,
                priceData.ProductUrl, cancellationToken);
        }

        return new ProductMatchingResult(productIncludingPrices.MapToDomain(), priceEntry, productAction,
            priceEntryAction);
    }

    private async Task<(PriceEntry PriceEntry, RequiredDatabaseAction Action)> FindOrCreateProductPriceEntryAsync(
        ProductDto product,
        RequiredDatabaseAction productAction,
        PriceData priceData,
        ICachedPriceEntryService cachedPriceEntryService)
    {
        if (productAction == RequiredDatabaseAction.Create)
        {
            return CreateNewPriceEntry(product, priceData);
        }

        var existingPriceEntry = product.PriceEntries?.FirstOrDefault(x => x.MarketId == priceData.MarketId)
            ?.MapToDomain();
        if (existingPriceEntry is not null)
        {
            return UpdateExistingPriceEntry(existingPriceEntry, priceData);
        }

        existingPriceEntry =
            (await cachedPriceEntryService.FindOneByAsync(x =>
                x.MarketId == priceData.MarketId && x.ProductId == product.Id));

        if (existingPriceEntry is not null)
        {
            return UpdateExistingPriceEntry(existingPriceEntry, priceData);
        }

        return CreateNewPriceEntry(product, priceData);
    }

    private (PriceEntry, RequiredDatabaseAction) UpdateExistingPriceEntry(PriceEntry priceEntry, PriceData priceData)
    {
        priceEntry.UpdateFrom(priceData);
        return (priceEntry, RequiredDatabaseAction.Update);
    }

    private (PriceEntry, RequiredDatabaseAction) CreateNewPriceEntry(ProductDto product, PriceData priceData)
    {
        var priceEntry = new PriceEntry
        {
            ProductId = product.Id,
            MarketId = priceData.MarketId,
            ProductUrl = priceData.ProductUrl,
            Price = priceData.Price,
            DiscountedPrice = priceData.DiscountedPrice,
            LastUpdated = DateTime.UtcNow,
        };

        product.PriceEntries ??= [];
        product.PriceEntries.Add(priceEntry.MapToDto());
        return (priceEntry, RequiredDatabaseAction.Create);
    }
}