using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedCategoryService : ICachedRepository<Category, string>, ICategoryService { }
