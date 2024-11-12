using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using API.Extensions;
using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;
using HtmlAgilityPack;

namespace API.Services.Crawlers;

/// <summary>
/// Base class for all market crawlers where <c><see cref="T"/></c> is a derived class.
/// </summary>
/// <typeparam name="T">Derived class</typeparam>
public abstract class BaseMarketCrawler<T> : IMarketCrawler
{
    protected readonly HttpClient HttpClient;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger<T> Logger;
    protected readonly IProductMatcher ProductMatcher;
    protected readonly Market Market;
    protected readonly Uri BaseUri;
    protected readonly string BaseUrl;
    protected TimeSpan DelayBetweenEachProductPageRequest { get; } = TimeSpan.FromMilliseconds(150);

    protected abstract string CategoryNodesXPath { get; }
    protected abstract string CategoryNameXPath { get; }
    protected abstract string CategoryNextPageButtonXPath { get; }
    protected abstract string ProductCardsXPath { get; }
    protected abstract string ProductCardImageXPath { get; }
    protected abstract string ProductCardNameXPath { get; }
    protected abstract string ProductCardPriceXPath { get; }
    protected abstract string ProductCardDiscountedPriceXPath { get; }
    protected abstract string ProductCardProductUrlXPath { get; }
    protected abstract string? ProductPageAttributeItemXPath { get; }
    protected abstract string? AttributeNameXPath { get; }
    protected abstract string? AttributeValueXPath { get; }

    public string GetBaseUrl => BaseUrl;
    public string MarketId => Market.Id;
    public MarketCrawlerSettings Settings { get; }

    protected BaseMarketCrawler(
        HttpClient httpClient,
        IServiceProvider serviceProvider,
        ILogger<T> logger,
        MarketCrawlersConfiguration crawlersConfiguration,
        string marketCrawlerKey,
        IProductMatcher productMatcher)
    {
        HttpClient = httpClient;
        ServiceProvider = serviceProvider;
        Logger = logger;
        ProductMatcher = productMatcher;

        var crawlerSettings = crawlersConfiguration.Crawlers.GetValueOrDefault(marketCrawlerKey);
        ArgumentNullException.ThrowIfNull(crawlerSettings);
        Settings = crawlerSettings;

        using var scope = serviceProvider.CreateScope();
        var marketService = scope.ServiceProvider.GetRequiredService<IMarketService>();
        var market = marketService.FindOneBy(m => m.Name.ToLower() == crawlerSettings.Name.ToLower());

        Market = market ?? marketService.Create(new Market
            { Name = crawlerSettings.Name, WebsiteUrl = crawlerSettings.BaseUrl });
        BaseUrl = Market.WebsiteUrl;
        BaseUri = new Uri(BaseUrl);
        HttpClient.BaseAddress = BaseUri;

        // Sites might reject requests from crawler if User-Agent is not set
        HttpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
    }

    public virtual async Task CrawlProductsAsync(bool parseProductPageInfo, CancellationToken cancellationToken)
    {
        var categories = await GetCategoryUrlsAsync(cancellationToken);
        var parsedProductsDict = new Dictionary<string, ProductParseResult>();

        var categoryCount = categories.Count;
        for (var i = 0; i < categoryCount; i++)
        {
            await CrawlCategoryAsync(parsedProductsDict, categories[i], i, categoryCount, cancellationToken);
        }

        Logger.LogInformation("Parsed products: {count}", parsedProductsDict?.Count);

        if (parseProductPageInfo && parsedProductsDict is not null)
        {
            Logger.LogInformation("Parsing products info from each product page");
            await ParseProductPagesAsync(parsedProductsDict, cancellationToken);
        }

        var productMatchingResults =
            await ProductMatcher.MatchOrCreateProductsAsync(parsedProductsDict?.Values.ToList() ?? [],
                cancellationToken);

        using var scope = ServiceProvider.CreateScope();
        var productManager = scope.ServiceProvider.GetRequiredService<IProductManager>();
        await productManager.UpsertProductsDataAsync(productMatchingResults, cancellationToken);
    }

    private async Task ParseProductPagesAsync(Dictionary<string, ProductParseResult> parsedProducts,
        CancellationToken cancellationToken)
    {
        var count = parsedProducts.Count;
        foreach (var (index, parsedProduct) in parsedProducts.Index())
        {
            Logger.LogInformation("Processing product page: {index}/{count}", index, count);
            var startTimestamp = Stopwatch.GetTimestamp();
            try
            {
                await ParseInfoFromProductPage(parsedProduct.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error parsing product page {Url}", parsedProduct.Value.PriceData.ProductUrl);
            }
        
            var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
            var timeDiff = DelayBetweenEachProductPageRequest - elapsedTime;
            if (timeDiff > TimeSpan.Zero)
            {
                await Task.Delay(timeDiff, cancellationToken);
            }
        }
    }

    protected virtual async Task<List<ProductCategory>> GetCategoryUrlsAsync(CancellationToken cancellationToken)
    {
        var homepage = await GetHtmlDocumentAsync(BaseUrl, cancellationToken);
        var categoryNodes = homepage?.DocumentNode.SelectNodes(CategoryNodesXPath);

        return (categoryNodes?
            .Select(node =>
            {
                var name = GetCategoryName(node);

                if (name.IsNullOrWhiteSpace())
                {
                    Logger.LogWarning("Could not get category name");
                    return null;
                }

                var categoryUrl = GetCategoryUrl(node);
                return new ProductCategory(name, categoryUrl);
            })
            .Where(x => x != null)
            .ToList() ?? [])!;
    }

    protected virtual async Task CrawlCategoryAsync(
        Dictionary<string, ProductParseResult> parsedProducts, ProductCategory category, int currentIndex,
        int categoryCount, CancellationToken cancellationToken)
    {
        int pageIteration = 1;
        int productsCount = 0;
        int parsedProductsCount = 0;
        bool hasNextPage = true;

        while (hasNextPage && cancellationToken.IsCancellationRequested == false)
        {
            var categoryUrl = $"{category.Url}?page={pageIteration}";
            var categoryPageDoc = await GetHtmlDocumentAsync(categoryUrl, cancellationToken);

            if (categoryPageDoc is null)
                break;

            var productCardNodes = categoryPageDoc.DocumentNode.SelectNodes(ProductCardsXPath);
            hasNextPage = HasCategoryNextPage(categoryPageDoc.DocumentNode);

            if (productCardNodes is null || productCardNodes.Count == 0)
            {
                Logger.LogWarning("Could not find any product on page '{page}'", pageIteration);
                hasNextPage = false;
                continue;
            }

            productsCount += productCardNodes.Count;

            foreach (var productCardNode in productCardNodes)
            {
                var (key, productParseResult) =
                    ParseCategoryProductFromNode(parsedProducts, productCardNode, category.Name, cancellationToken);

                if (key is not null && productParseResult is not null)
                {
                    parsedProductsCount++;
                    parsedProducts.Add(key, productParseResult);
                    continue;
                }

                Logger.LogError("Could not add product of category '{category}'", category.Name);
            }

            if (hasNextPage)
            {
                pageIteration++;
            }
        }

        Logger.LogInformation(
            "Category '{cat}' ({catIndex}/{catCount}), pages '{page}', products parsed '{parsedProdCount}/{prodCount}'",
            category.Name, currentIndex + 1, categoryCount, pageIteration, parsedProductsCount, productsCount);
    }

    protected virtual string? GetCategoryName(HtmlNode categoryNode)
    {
        return categoryNode.SelectSingleNode(CategoryNameXPath)?.InnerText;
    }

    protected virtual string GetCategoryUrl(HtmlNode categoryNode)
    {
        return new Uri(BaseUri, categoryNode.GetAttributeValue("href", "")).ToString();
    }

    protected virtual bool HasCategoryNextPage(HtmlNode documentNode)
    {
        return documentNode.SelectSingleNode(CategoryNextPageButtonXPath) is not null;
    }

    protected virtual (string? RelativeHrefKey, ProductParseResult? productParseResult) ParseCategoryProductFromNode(
        Dictionary<string, ProductParseResult> parsedProducts, HtmlNode productCardNode, string category,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return (null, null);
        }

        var productRelativeHrefNode = productCardNode.SelectSingleNode(ProductCardProductUrlXPath);
        if (productRelativeHrefNode is null)
        {
            Logger.LogError("Product's relative href node is null");
            return (null, null);
        }

        var productRelativeHref = GetProductRelativeHref(productRelativeHrefNode);
        if (productRelativeHref.IsNullOrWhiteSpace())
        {
            Logger.LogError("Could not get product href from node");
            return (null, null);
        }

        if (parsedProducts.ContainsKey(productRelativeHref))
        {
            return (null, null);
        }

        var productUrl = new Uri(BaseUri, productRelativeHref).ToString();

        var imageNode = productCardNode.SelectSingleNode(ProductCardImageXPath);
        if (imageNode is null)
        {
            Logger.LogWarning("Product's image node is null");
        }

        var imageUrl = imageNode is null ? string.Empty : GetProductCardImageUrl(imageNode);

        var nameNode = productCardNode.SelectSingleNode(ProductCardNameXPath);
        if (nameNode is null)
        {
            Logger.LogError("Product's name node is null");
            return (null, null);
        }

        var name = GetProductCardName(nameNode);
        if (name.IsNullOrWhiteSpace())
        {
            Logger.LogError("Product's name is null");
            return (null, null);
        }

        var (price, discountedPrice) = GetProductCardPrices(productCardNode);
        if (price is null)
        {
            return (null, null);
        }

        var priceData = new PriceData
        {
            Price = price.Value,
            DiscountedPrice = discountedPrice,
            MarketId = Market.Id,
            ProductUrl = productUrl,
        };

        var productToMatch = new ProductToMatch
        {
            Name = name,
            Category = category,
            Description = string.Empty,
            ImageUrl = imageUrl,
            Attributes = []
        };

        var productParseResult = new ProductParseResult(productToMatch, priceData);
        return (productRelativeHref, productParseResult);
    }

    protected virtual (decimal? Price, decimal? DiscountedPrice) GetProductCardPrices(HtmlNode productCardNode)
    {
        var priceWasParsed =
            TryGetAndParsePriceFromCardNode(productCardNode, ProductCardPriceXPath, true, out var price);
        if (priceWasParsed == false)
        {
            return (null, null);
        }

        var discountedPriceWasParsed = TryGetAndParsePriceFromCardNode(productCardNode, ProductCardDiscountedPriceXPath,
            false, out var discountedPrice);

        return (price, discountedPriceWasParsed ? discountedPrice : null);
    }

    protected bool TryGetAndParsePriceFromCardNode(HtmlNode productCardNode, string priceXPath, bool useLogs,
        out decimal price)
    {
        price = 0;
        var priceNode = productCardNode.SelectSingleNode(priceXPath);
        if (priceNode is null)
        {
            if (useLogs) Logger.LogError("Product's price node is null");
            return false;
        }

        var priceStr = GetProductCardPriceAsString(priceNode);
        if (priceStr.IsNullOrWhiteSpace())
        {
            if (useLogs) Logger.LogError("Could not get product price from node");
            return false;
        }

        if (decimal.TryParse(priceStr, CultureInfo.InvariantCulture, out var parsedPrice) == false)
        {
            if (useLogs) Logger.LogError("Could not parse product price '{priceStr}' to decimal", priceStr);
            return false;
        }

        price = parsedPrice;
        return true;
    }

    protected virtual string GetProductRelativeHref(HtmlNode productRelativeHrefNode)
    {
        return productRelativeHrefNode.GetAttributeValue("href", string.Empty);
    }

    protected virtual string GetProductCardImageUrl(HtmlNode imageNode)
    {
        return imageNode.Attributes["srcset"]?.Value ?? string.Empty;
    }

    protected virtual string? GetProductCardName(HtmlNode productCardNameNode)
    {
        return productCardNameNode.InnerText?.Trim().Replace("  ", " ");
    }

    protected virtual string? GetProductCardPriceAsString(HtmlNode priceNode)
    {
        return priceNode.Attributes["value"]?.Value.Replace(',', '.');
    }

    protected virtual async Task ParseInfoFromProductPage(ProductParseResult productParseResult,
        CancellationToken cancellationToken)
    {
        if (ProductPageAttributeItemXPath.IsNullOrWhiteSpace() || AttributeNameXPath.IsNullOrWhiteSpace() ||
            AttributeValueXPath.IsNullOrWhiteSpace())
        {
            return;
        }

        var productPageDoc = await GetHtmlDocumentAsync(productParseResult.PriceData.ProductUrl, cancellationToken);
        var attributeNodes = productPageDoc?.DocumentNode.SelectNodes(ProductPageAttributeItemXPath);

        if (attributeNodes is null)
            return;

        foreach (var attributeNode in attributeNodes)
        {
            var (name, value) = GetProductPageAttribute(attributeNode.SelectSingleNode(AttributeNameXPath),
                attributeNode.SelectSingleNode(AttributeValueXPath));

            if (name.IsNullOrWhiteSpace() == false && value.IsNullOrWhiteSpace() == false)
            {
                productParseResult.ProductToMatch.Attributes.Add(name, value);
            }
        }
    }

    protected virtual (string? Name, string? Value) GetProductPageAttribute(HtmlNode? attributeNameNode,
        HtmlNode? attributeValueNode)
    {
        return (attributeNameNode?.InnerText?.Trim(), attributeValueNode?.InnerText?.Trim());
    }

    protected async Task<HtmlDocument?> GetHtmlDocumentAsync(string url, CancellationToken cancellationToken)
    {
        using var response =
            await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.IsSuccessStatusCode == false)
        {
            Logger.LogError("Failed to fetch document from '{Url}': {StatusCode}", url, response.StatusCode);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(stream);
        return htmlDocument;
    }
}