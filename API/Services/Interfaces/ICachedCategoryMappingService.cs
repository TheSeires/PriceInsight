using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedCategoryMappingService
    : ICachedRepository<CategoryMapping, string>,
        ICategoryMappingService { }
