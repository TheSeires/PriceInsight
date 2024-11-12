using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;

namespace API.Services.Repositories;

public class CachedPriceEntryService : BaseCachedRepository<IPriceEntryService, PriceEntry, string>,
    ICachedPriceEntryService
{
    private const string _prefixKey = nameof(PriceEntry);

    public CachedPriceEntryService(IPriceEntryService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache)
    {
    }
}