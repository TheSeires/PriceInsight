using Microsoft.Extensions.Localization;

namespace API.Models.Application;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _basePath;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JsonStringLocalizerFactory(
        string basePath,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _basePath = basePath;
        _loggerFactory = loggerFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer(
            _basePath,
            _loggerFactory.CreateLogger<JsonStringLocalizer>(),
            _httpContextAccessor
        );
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer(
            _basePath,
            _loggerFactory.CreateLogger<JsonStringLocalizer>(),
            _httpContextAccessor
        );
    }
}
