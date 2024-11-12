using API.Models.Database;

namespace API.Models.Application;

public record ProductMatchingResult(
    Product ProductDto,
    PriceEntry PriceEntry,
    RequiredDatabaseAction ProductAction,
    RequiredDatabaseAction PriceEntryAction);