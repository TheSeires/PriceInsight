using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedMarketService : ICachedRepository<Market, string>, IMarketService
{
}