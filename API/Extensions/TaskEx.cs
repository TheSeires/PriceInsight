namespace API.Extensions;

public static class TaskEx
{
    public static async Task AsSafeExecutionAsync(this Task task, ILogger logger)
    {
        try
        {
            await task;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}