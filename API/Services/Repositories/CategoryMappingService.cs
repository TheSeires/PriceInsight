using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class CategoryMappingService
    : BaseRepositoryService<CategoryMapping, string>,
        ICategoryMappingService
{
    protected override IMongoCollection<CategoryMapping> Collection { get; init; }

    public CategoryMappingService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<CategoryMapping>(
            DbCollectionNames.CategoryMappingCollectionName
        );
    }

    public async Task<string?> GetTargetCategoryIdAsync(
        string sourceCategory,
        string sourceMarketId,
        CancellationToken cancellationToken = default
    )
    {
        var categoryMapping = await Collection
            .Find(m => m.SourceCategory == sourceCategory && m.SourceMarketId == sourceMarketId)
            .FirstOrDefaultAsync(cancellationToken);

        return categoryMapping?.TargetCategoryId;
    }
}
