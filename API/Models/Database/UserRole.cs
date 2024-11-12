using API.Models.Application;
using API.Models.Database.Interfaces;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.UserRoleCollectionName)]
public class UserRole : MongoIdentityRole<Guid>, IEntity<Guid>
{
    public UserRole(string roleName)
        : base(roleName) { }

    public UserRole()
        : base() { }
}
