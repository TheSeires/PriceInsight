using API.Models.Application;
using API.Services.Interfaces;
using HtmlAgilityPack;

namespace API.Services.Crawlers;

public class AtbCrawler : BaseMarketCrawler<AtbCrawler>
{
    protected override string CategoryNodesXPath =>
        "//ul[@class='category-menu']/li[contains(@class, 'category-menu__item') and not(contains(@class, 'category-menu__item--not-dropdown'))]/a";

    protected override string CategoryNameXPath => ".//span[@class='category-menu__link']";

    protected override string CategoryNextPageButtonXPath =>
        "//nav[@class='product-pagination__nav']//li[contains(@class, 'next') and not(contains(@class, 'disabled'))]";

    protected override string ProductCardsXPath =>
        "//article[contains(@class, 'catalog-item') and contains(@class, 'js-product-container')]";

    protected override string ProductCardImageXPath =>
        ".//div[contains(@class, 'catalog-item__photo')]//picture//source";

    protected override string ProductCardNameXPath => ".//div[contains(@class, 'catalog-item__title')]";

    protected override string ProductCardPriceXPath => ".//data[@class='product-price__top']";

    protected override string ProductCardDiscountedPriceXPath => ".//data[@class='product-price__bottom']";

    protected override string ProductCardProductUrlXPath => ".//div[contains(@class, 'catalog-item__title')]//a";

    protected override string ProductPageAttributeItemXPath =>
        ".//div[@id='productCharacteristics']//div[contains(@class, 'product-characteristics__item')]";

    protected override string AttributeNameXPath => ".//div[@class='product-characteristics__name']";

    protected override string AttributeValueXPath => ".//div[@class='product-characteristics__value']";


    public AtbCrawler(
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider,
        ILogger<AtbCrawler> logger,
        MarketCrawlersConfiguration crawlersConfiguration,
        IProductMatcher productMatcher
    )
        : base(
            httpClientFactory.CreateClient(HttpClients.Simple),
            serviceProvider,
            logger,
            crawlersConfiguration,
            MarketCrawlerKeys.ATB,
            productMatcher)
    {
    }

    protected override (decimal? Price, decimal? DiscountedPrice) GetProductCardPrices(HtmlNode productCardNode)
    {
        var (price, discountedPrice) = base.GetProductCardPrices(productCardNode);
        if (price is not null && discountedPrice is not null)
        {
            // Swap it around to make it relevant to the way it is displayed on the site
            return (discountedPrice, price);
        }

        return (price, null);
    }
}