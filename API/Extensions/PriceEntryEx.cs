using API.Models.Application;
using API.Models.Database;
using API.Models.Dto;
using API.Services.Interfaces;

namespace API.Extensions;

public static class PriceEntryEx
{
    public static async Task IncludeMarketsAsync(this List<PriceEntryDto>? priceEntries, IMarketService marketService,
        CancellationToken cancellationToken)
    {
        if (priceEntries is null)
            return;

        var marketIds = priceEntries.Select(x => x.MarketId).ToArray();

        if (marketIds.Length == 0)
            return;

        var markets =
            await marketService.FindByAsync(x => marketIds.Contains(x.Id), cancellationToken);

        foreach (var priceEntry in priceEntries)
        {
            var market = markets.FirstOrDefault(x => x.Id == priceEntry.MarketId);
            if (market is null)
                continue;

            priceEntry.Market = market;
        }
    }

    public static void UpdateFrom(this PriceEntry priceEntry, PriceData priceData)
    {
        priceEntry.Price = priceData.Price;
        priceEntry.DiscountedPrice = priceData.DiscountedPrice;
        priceEntry.ProductUrl = priceData.ProductUrl;
        priceEntry.LastUpdated = DateTime.UtcNow;
    }
}