using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class ProductFollowingService : BaseRepositoryService<ProductFollowing, string>, IProductFollowingService
{
    protected override IMongoCollection<ProductFollowing> Collection { get; init; }

    public ProductFollowingService(IMongoDatabase database) : base(database)
    {
        Collection = database.GetCollection<ProductFollowing>(DbCollectionNames.ProductFollowingCollectionName);
    }
}