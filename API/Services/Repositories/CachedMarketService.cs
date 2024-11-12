using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;

namespace API.Services.Repositories;

public class CachedMarketService : BaseCachedRepository<IMarketService, Market, string>,
    ICachedMarketService
{
    private const string _prefixKey = nameof(Market);

    public CachedMarketService(IMarketService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache)
    {
    }
}