using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Repositories;

public class CachedCategoryMappingService
    : BaseCachedRepository<ICategoryMappingService, CategoryMapping, string>,
        ICachedCategoryMappingService
{
    private const string _prefixKey = nameof(CategoryMapping);

    public CachedCategoryMappingService(ICategoryMappingService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache) { }

    private string GetKey(string category, string marketId)
    {
        return $"{_prefixKey}:[category]={category}:[marketId]={marketId}";
    }

    public Task<string?> GetTargetCategoryIdAsync(
        string sourceCategory,
        string sourceMarketId,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(sourceCategory, sourceMarketId);
        return Cache.GetOrAddAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(CacheDuration);
                return Decorated.GetTargetCategoryIdAsync(
                    sourceCategory,
                    sourceMarketId,
                    cancellationToken
                );
            }
        );
    }
}
