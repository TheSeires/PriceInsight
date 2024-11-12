using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedPriceEntryService : ICachedRepository<PriceEntry, string> { }
