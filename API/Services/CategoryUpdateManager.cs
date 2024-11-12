using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Bson;

namespace API.Services;

public class CategoryUpdateManager : ICategoryUpdateManager
{
    private readonly ICategoryMappingService _cachedCategoryMappingService;
    private readonly IProductService _productService;
    private readonly ICategorizationIssueService _cachedCategorizationIssueService;
    private readonly ILogger<CategoryUpdateManager> _logger;

    public CategoryUpdateManager(
        ICachedCategoryMappingService cachedCategoryMappingService,
        ICachedCategorizationIssueService categorizationIssueService,
        IProductService productService,
        ILogger<CategoryUpdateManager> logger)
    {
        _cachedCategoryMappingService = cachedCategoryMappingService;
        _cachedCategorizationIssueService = categorizationIssueService;
        _productService = productService;
        _logger = logger;
    }

    public async Task UpdateCategoriesAsync(CancellationToken cancellationToken)
    {
        var categoryMappings =
            await _cachedCategoryMappingService.FindByAsync(null, cancellationToken: cancellationToken);
        var categMappingDict = categoryMappings.ToDictionary(
            m => (m.SourceMarketId, m.SourceCategory),
            m => m.TargetCategoryId);

        var productsWithoutCategory = await _productService.FindByAsync(x =>
            x.CategoryId == ObjectId.Empty.ToString(), cancellationToken);
        var productsToUpdate = new List<Product>();

        foreach (var product in productsWithoutCategory)
        {
            if (
                categMappingDict.TryGetValue(
                    (product.SourceMarketId, product.SourceCategory),
                    out var targetCategId
                ) == false
            )
                continue;

            product.CategoryId = targetCategId;
            productsToUpdate.Add(product);
        }

        await _productService.BulkUpdateAsync(
            productsToUpdate,
            cancellationToken: cancellationToken
        );

        var categIssuesToDelete = await _cachedCategorizationIssueService.FindByAsync(i =>
            categoryMappings.Any(m =>
                m.SourceMarketId == i.MarketId && m.SourceCategory == i.SourceCategory
            ), cancellationToken);

        await _cachedCategorizationIssueService.DeleteManyAsync(
            categIssuesToDelete,
            cancellationToken
        );

        _logger.LogInformation(
            "Updated {ProductCount} products and deleted {IssueCount} categorization issues.",
            productsToUpdate.Count,
            categIssuesToDelete.Count
        );
    }
}