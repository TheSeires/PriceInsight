using API.Models.Application;

namespace API.Services.Interfaces;

public interface IMarketCrawler
{
    string GetBaseUrl { get; }
    string MarketId { get; }
    MarketCrawlerSettings Settings { get; }
    Task CrawlProductsAsync(bool parseProductPageInfo, CancellationToken cancellationToken);
}
