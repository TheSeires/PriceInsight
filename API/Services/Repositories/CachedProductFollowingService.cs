using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;

namespace API.Services.Repositories;

public class CachedProductFollowingService : BaseCachedRepository<IProductFollowingService, ProductFollowing, string>,
    ICachedProductFollowingService
{
    private const string _prefixKey = nameof(ProductFollowing);

    public CachedProductFollowingService(IProductFollowingService decorated, IAppCache cache) : base(_prefixKey,
        decorated, cache)
    {
    }
}