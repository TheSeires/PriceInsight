using API.Models.Database;
using API.Services.Interfaces;
using LazyCache;

namespace API.Services.Repositories;

public class CachedCategoryService
    : BaseCachedRepository<ICategoryService, Category, string>,
        ICachedCategoryService
{
    private const string _prefixKey = nameof(Category);

    public CachedCategoryService(ICategoryService decorated, IAppCache cache)
        : base(_prefixKey, decorated, cache) { }
}
