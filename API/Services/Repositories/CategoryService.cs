using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class CategoryService : BaseRepositoryService<Category, string>, ICategoryService
{
    protected override IMongoCollection<Category> Collection { get; init; }

    public CategoryService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<Category>(DbCollectionNames.CategoryCollectionName);
    }
}
