using API.Models.Application;
using API.Services.Interfaces;

namespace API.Services.Background;

public class CategoryUpdaterService : BackgroundService
{
    private readonly TimeSpan ExecutePeriod = TimeSpan.FromMinutes(30);
    private readonly TimeSpan RetryPeriod = TimeSpan.FromMinutes(1);

    private readonly IServiceStateTracker _stateTracker;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CategoryUpdaterService> _logger;
    private readonly Lock _delayLock = new();

    private CancellationTokenSource? _delayCts;

    public CategoryUpdaterService(
        IServiceStateTracker stateTracker,
        IServiceProvider serviceProvider,
        ILogger<CategoryUpdaterService> logger)
    {
        _stateTracker = stateTracker;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public EmptyResult<string> SkipDelay()
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
        }

        return EmptyResult.FromSuccess<string>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            if (_stateTracker.GetServiceState<MarketCrawlersStarter>() == ServiceState.Idle)
            {
                await ExecuteUpdateAsync(stoppingToken);
                await DelayNextExecutionAsync(stoppingToken);
            }
            else
            {
                await Task.Delay(RetryPeriod, stoppingToken);
            }
        }
    }

    private async Task ExecuteUpdateAsync(CancellationToken stoppingToken)
    {
        _stateTracker.SetServiceState<CategoryUpdaterService>(ServiceState.Running);
        _logger.LogInformation("Category update task started.");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var categoryUpdateManager =
                scope.ServiceProvider.GetRequiredService<ICategoryUpdateManager>();

            await categoryUpdateManager.UpdateCategoriesAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Category update was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during category update.");
        }

        _stateTracker.SetServiceState<CategoryUpdaterService>(ServiceState.Idle);
        _logger.LogInformation("Category update task completed.");
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
}