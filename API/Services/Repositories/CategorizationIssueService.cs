using System.Linq.Expressions;
using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class CategorizationIssueService
    : BaseRepositoryService<CategorizationIssue, string>,
        ICategorizationIssueService
{
    protected override IMongoCollection<CategorizationIssue> Collection { get; init; }

    public CategorizationIssueService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<CategorizationIssue>(
            DbCollectionNames.CategorizationIssueCollectionName
        );
    }

    public async Task<CategorizationIssue> CreateIfNotExistsAsync(
        string sourceCategory,
        string marketId,
        string sourceProductUrl,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<CategorizationIssue, bool>> expression = x =>
            x.SourceCategory == sourceCategory && x.MarketId == marketId;

        var existingIssue = await FindOneByAsync(expression, cancellationToken);
        if (existingIssue is not null)
        {
            return existingIssue;
        }

        var newIssue = new CategorizationIssue
        {
            SourceProductUrl = sourceProductUrl,
            SourceCategory = sourceCategory,
            MarketId = marketId,
        };

        return await CreateAsync(newIssue, cancellationToken);
    }
}
