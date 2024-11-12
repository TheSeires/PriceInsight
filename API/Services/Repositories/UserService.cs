using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class UserService : BaseRepositoryService<User, Guid>, IUserService
{
    protected override IMongoCollection<User> Collection { get; init; }

    public UserService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<User>(DbCollectionNames.UserCollectionName);
    }
}
