using System.Text.RegularExpressions;
using API.Models.Application;
using API.Services.Interfaces;
using HtmlAgilityPack;

namespace API.Services.Crawlers;

public class RukavychkaCrawler : BaseMarketCrawler<RukavychkaCrawler>
{
    protected override string CategoryNodesXPath =>
        "//div[contains(@class, 'fm-category-wall-box')]//div[contains(@class, 'fm-category-wall-item-info')]";

    protected override string CategoryNameXPath => "./a[contains(@class, 'fm-category-wall-item-title')]/span";

    protected override string CategoryNextPageButtonXPath =>
        "//div[contains(@class, 'fm-category-content')]/ul[contains(@class, 'pagination')]/li[last()]";

    protected override string ProductCardsXPath =>
        "//div[@id='content' and contains(@class, 'fm-category-content')]//div[contains(@class, 'fm-module-item')]";

    protected override string ProductCardImageXPath =>
        "./div[contains(@class, 'fm-module-img')]//img[contains(@class, 'img-fluid')]";

    protected override string ProductCardNameXPath =>
        "./div[contains(@class, 'fm-category-product-caption')]/div[contains(@class, 'fm-module-title')]/a";

    protected override string ProductCardPriceXPath =>
        "./div[@class='fm-category-product-caption']//span[contains(@class, 'fm-module-price-new')]";

    protected override string ProductCardDiscountedPriceXPath =>
        "./div[@class='fm-category-product-caption']//span[contains(@class, 'fm-module-price-old')]";

    protected override string ProductCardProductUrlXPath =>
        "./div[contains(@class, 'fm-category-product-caption')]/div[contains(@class, 'fm-module-title')]/a";

    protected override string? ProductPageAttributeItemXPath => null;

    protected override string? AttributeNameXPath => null;

    protected override string? AttributeValueXPath => null;

    private const string PriceRegEx = @"\d+(\.\d+)?";

    public RukavychkaCrawler(
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider,
        ILogger<RukavychkaCrawler> logger,
        MarketCrawlersConfiguration crawlersConfiguration,
        IProductMatcher productMatcher
    )
        : base(
            httpClientFactory.CreateClient(HttpClients.Simple),
            serviceProvider,
            logger,
            crawlersConfiguration,
            MarketCrawlerKeys.Rukavychka,
            productMatcher)
    {
    }

    protected override bool HasCategoryNextPage(HtmlNode documentNode)
    {
        var nextPageNode = documentNode.SelectSingleNode(CategoryNextPageButtonXPath);
        return nextPageNode is not null && nextPageNode.HasClass("active") == false;
    }

    protected override string GetCategoryUrl(HtmlNode categoryNode)
    {
        var linkNode = categoryNode.SelectSingleNode("./a");
        return linkNode?.GetAttributeValue("href", "") ?? string.Empty;
    }

    protected override string GetProductCardPriceAsString(HtmlNode priceNode)
    {
        return Regex.Match(priceNode.InnerText, PriceRegEx).Value;
    }

    protected override string GetProductCardImageUrl(HtmlNode imageNode)
    {
        return imageNode.GetAttributeValue("src", "");
    }
}