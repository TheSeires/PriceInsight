using API.Models.Application;
using API.Models.Database.Interfaces;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace API.Models.Database;

[CollectionName(DbCollectionNames.UserCollectionName)]
public class User : MongoIdentityUser<Guid>, IEntity<Guid> { }
