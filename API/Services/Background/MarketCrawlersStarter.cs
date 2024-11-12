using API.Models.Application;
using API.Models.Database;
using API.Services.Interfaces;

namespace API.Services.Background;

public class MarketCrawlersStarter : BackgroundService
{
    public readonly TimeSpan ExecutePeriod = TimeSpan.FromMinutes(30);
    public readonly TimeSpan MarketUpdatePeriod = TimeSpan.FromHours(6);
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketCrawlersStarter> _logger;
    private readonly IServiceStateTracker _stateTracker;
    private readonly Lock _delayLock = new();

    private CancellationTokenSource? _delayCts;
    private bool _forceStart;
    private bool _parseProductPageInfo = true;

    public MarketCrawlersStarter(
        IServiceProvider serviceProvider,
        ILogger<MarketCrawlersStarter> logger,
        IServiceStateTracker stateTracker)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _stateTracker = stateTracker;
    }

    public EmptyResult<string> SkipDelay(bool parseProductPageInfo = true)
    {
        if (_delayCts is null)
            return "Delay cancellation token was null";

        var serviceState = _stateTracker.GetServiceState<MarketCrawlersStarter>();
        if (serviceState == ServiceState.Running)
        {
            return "Market crawlers are already running";
        }

        lock (_delayLock)
        {
            _delayCts.Cancel();
            _forceStart = true;
            _parseProductPageInfo = parseProductPageInfo;
        }

        return EmptyResult.FromSuccess<string>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            _stateTracker.SetServiceState<MarketCrawlersStarter>(ServiceState.Running);

            using var scope = _serviceProvider.CreateScope();
            var marketCrawlers = scope.ServiceProvider.GetServices<IMarketCrawler>().ToArray();
            _logger.LogInformation("Market crawlers found: {crawlers}",
                string.Join(", ", marketCrawlers.Select(x => x.Settings.Name)));

            var forceUpdate = false;
            lock (_delayLock)
            {
                if (_forceStart)
                {
                    forceUpdate = true;
                    _forceStart = false;
                }
            }

            await ProcessMarketCrawlersAsync(marketCrawlers, scope.ServiceProvider, forceUpdate, stoppingToken);

            _stateTracker.SetServiceState<MarketCrawlersStarter>(ServiceState.Idle);
            await DelayNextExecutionAsync(stoppingToken);
        }
    }

    private async Task ProcessMarketCrawlersAsync(IMarketCrawler[] marketCrawlers,
        IServiceProvider scopedServiceProvider, bool forceUpdate, CancellationToken stoppingToken)
    {
        var crawlerHistoryService = scopedServiceProvider.GetRequiredService<ICrawlerHistoryService>();

        foreach (var marketCrawler in marketCrawlers)
        {
            var crawlerHistory = await crawlerHistoryService.FindOneByAsync(
                x => x.MarketId == marketCrawler.MarketId,
                stoppingToken);

            if (forceUpdate == false && IsCrawlerUpToDate(marketCrawler, crawlerHistory))
            {
                continue;
            }

            await CrawlMarketProductsAsync(marketCrawler, crawlerHistory, crawlerHistoryService, forceUpdate,
                stoppingToken);
        }
    }

    private bool IsCrawlerUpToDate(IMarketCrawler marketCrawler,
        CrawlerHistory? crawlerHistory)
    {
        if (crawlerHistory is not null && crawlerHistory.Updated > DateTime.UtcNow - MarketUpdatePeriod)
        {
            _logger.LogInformation("Crawler for '{name}' market is up to date", marketCrawler.Settings.Name);
            return true;
        }

        return false;
    }

    private async Task CrawlMarketProductsAsync(IMarketCrawler crawler, CrawlerHistory? crawlerHistory,
        ICrawlerHistoryService crawlerHistoryService, bool forceUpdate, CancellationToken stoppingToken)
    {
        var startTime = DateTime.Now;
        _logger.LogInformation("Started crawling products from '{name}' at '{time}'", crawler.Settings.Name,
            TimeOnly.FromDateTime(startTime));

        var parseProductInfo = forceUpdate == false || _parseProductPageInfo;
        var crawlTask = crawler.CrawlProductsAsync(parseProductInfo, stoppingToken);
        _ = LogCrawlProgressAsync(crawler.Settings.Name, startTime, crawlTask, stoppingToken);

        await crawlTask;

        await CreateOrUpdateCrawlerHistoryAsync(crawler, crawlerHistory, crawlerHistoryService, stoppingToken);

        var endTime = DateTime.Now;
        _logger.LogInformation("Finished crawling products from '{name}' at '{time}'. Elapsed time: {elapsedTime}",
            crawler.Settings.Name,
            TimeOnly.FromDateTime(endTime),
            (endTime - startTime).ToString(@"hh\:mm\:ss"));
    }

    private async Task LogCrawlProgressAsync(string crawlerName, DateTime startTime, Task crawlTask,
        CancellationToken stoppingToken)
    {
        while (crawlTask.IsCompleted == false && stoppingToken.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            if (crawlTask.IsCompleted || stoppingToken.IsCancellationRequested)
                break;

            var elapsedTime = DateTime.Now - startTime;
            _logger.LogInformation("Crawling '{name}' in progress. Elapsed time: {elapsed}",
                crawlerName, elapsedTime.ToString(@"hh\:mm\:ss"));
        }
    }

    private async Task CreateOrUpdateCrawlerHistoryAsync(IMarketCrawler crawler, CrawlerHistory? crawlerHistory,
        ICrawlerHistoryService crawlerHistoryService, CancellationToken stoppingToken)
    {
        var utcNow = DateTime.UtcNow;
        if (crawlerHistory is null)
        {
            var newCrawlerHistory = new CrawlerHistory
            {
                Created = utcNow,
                Updated = utcNow,
                MarketId = crawler.MarketId,
            };

            await crawlerHistoryService.CreateAsync(newCrawlerHistory, stoppingToken);
        }
        else
        {
            crawlerHistory.Updated = utcNow;
            await crawlerHistoryService.UpdateAsync(crawlerHistory.Id, crawlerHistory, stoppingToken);
        }
    }

    private async Task DelayNextExecutionAsync(CancellationToken stoppingToken)
    {
        _delayCts = new CancellationTokenSource();
        var delayTask = Task.Delay(ExecutePeriod, _delayCts.Token);
        var completedDelayTask = await Task.WhenAny(delayTask, Task.Delay(Timeout.Infinite, stoppingToken));

        if (completedDelayTask == delayTask && delayTask.IsCanceled == false)
        {
            await delayTask;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        try
        {
            _delayCts?.Cancel();
            _delayCts?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Handle exception when cancellation token source is already disposed
        }
    }
}