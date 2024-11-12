using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class CrawlerHistoryService
    : BaseRepositoryService<CrawlerHistory, string>,
        ICrawlerHistoryService
{
    protected override IMongoCollection<CrawlerHistory> Collection { get; init; }

    public CrawlerHistoryService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<CrawlerHistory>(
            DbCollectionNames.CrawlersHistoryCollectionName
        );
    }
}
