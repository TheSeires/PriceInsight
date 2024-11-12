using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICategorizationIssueService : IRepositoryService<CategorizationIssue, string>
{
    Task<CategorizationIssue> CreateIfNotExistsAsync(
        string sourceCategory,
        string marketId,
        string sourceProductUrl,
        CancellationToken cancellationToken
    );
}
