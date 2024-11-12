using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using MongoDB.Driver;

namespace API.Services.Repositories;

public class PriceEntryService : BaseRepositoryService<PriceEntry, string>, IPriceEntryService
{
    protected override IMongoCollection<PriceEntry> Collection { get; init; }

    public PriceEntryService(IMongoDatabase database)
        : base(database)
    {
        Collection = database.GetCollection<PriceEntry>(DbCollectionNames.PriceEntryCollectionName);
    }
}