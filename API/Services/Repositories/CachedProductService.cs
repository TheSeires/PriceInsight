using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;

namespace API.Services.Repositories;

public class CachedProductService
    : BaseCachedRepository<IProductService, Product, string>,
        ICachedProductService
{
    private const string _prefixKey = nameof(Product);

    public CachedProductService(IProductService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache) { }
}
