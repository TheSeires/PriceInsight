using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class ProductService : BaseRepositoryService<Product, string>, IProductService
{
    protected override IMongoCollection<Product> Collection { get; init; }

    public ProductService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<Product>(DbCollectionNames.ProductCollectionName);
    }
}
