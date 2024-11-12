using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Repositories;

public class CachedCategorizationIssueService
    : BaseCachedRepository<ICategorizationIssueService, CategorizationIssue, string>,
        ICachedCategorizationIssueService
{
    private const string _prefixKey = nameof(CategorizationIssue);

    public CachedCategorizationIssueService(ICategorizationIssueService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache) { }

    private string GetKey(string category, string marketId)
    {
        return $"{_prefixKey}:[category]={category}:[marketId]={marketId}";
    }

    public Task<CategorizationIssue> CreateIfNotExistsAsync(
        string sourceCategory,
        string marketId,
        string sourceProductUrl,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(sourceCategory, marketId);
        return Cache.GetOrAddAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(FilteredDataCacheDuration);
                return Decorated.CreateIfNotExistsAsync(
                    sourceCategory,
                    marketId,
                    sourceProductUrl,
                    cancellationToken
                );
            }
        );
    }
}
