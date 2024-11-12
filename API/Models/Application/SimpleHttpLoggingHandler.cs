using System.Diagnostics;

namespace API.Models.Application;

public class SimpleHttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<SimpleHttpLoggingHandler> _logger;

    public SimpleHttpLoggingHandler(ILogger<SimpleHttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        _logger.LogInformation("Sending {method} request to: {uri}", request.Method.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);
        var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
        _logger.LogInformation("Received response after {elapsedTime} - {statusCode}", elapsedTime,
            response.StatusCode);

        return response;
    }
}