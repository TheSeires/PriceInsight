using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICategoryMappingService : IRepositoryService<CategoryMapping, string>
{
    Task<string?> GetTargetCategoryIdAsync(
        string sourceCategory,
        string sourceMarketId,
        CancellationToken cancellationToken = default
    );
}
