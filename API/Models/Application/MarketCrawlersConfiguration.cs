namespace API.Models.Application;

public class MarketCrawlersConfiguration
{
    public Dictionary<string, MarketCrawlerSettings> Crawlers { get; init; } = [];

    public MarketCrawlersConfiguration(Dictionary<string, MarketCrawlerSettings> crawlers)
    {
        Crawlers = crawlers;
    }
}
