using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class MarketService : BaseRepositoryService<Market, string>, IMarketService
{
    protected override IMongoCollection<Market> Collection { get; init; }

    public MarketService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<Market>(DbCollectionNames.MarketCollectionName);
    }
}
