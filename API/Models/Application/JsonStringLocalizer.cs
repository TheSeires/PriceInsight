using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;

namespace API.Models.Application;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly string _basePath;
    private readonly ILogger<JsonStringLocalizer> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, JObject> _localizationCache;

    public JsonStringLocalizer(
        string basePath,
        ILogger<JsonStringLocalizer> logger,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _basePath = basePath;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _localizationCache = new ConcurrentDictionary<string, JObject>();
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = GetString(name) ?? name;

            for (int i = 0; i < arguments.Length; i++)
            {
                var props = arguments[i].GetType().GetProperties();

                foreach (var prop in props)
                {
                    if (prop is not null)
                    {
                        string propName = prop.Name;
                        string? propValue = prop.GetValue(arguments[i])?.ToString();
                        value = value.Replace($"{{{{{propName}}}}}", propValue);
                    }
                }
            }

            return new LocalizedString(name, value, resourceNotFound: value == null);
        }
    }

    private string? GetString(string name)
    {
        var culture = GetCultureFromCookie() ?? CultureInfo.CurrentUICulture.Name;
        var filePath = Path.Combine(_basePath, $"{culture}.json");

        var jsonObject = _localizationCache.GetOrAdd(
            culture,
            _ =>
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JObject.Parse(json);
                }

                _logger.LogWarning($"Localization file not found: {filePath}");
                return new JObject();
            }
        );

        return jsonObject.SelectToken(name)?.ToString();
    }

    private string? GetCultureFromCookie()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var cookieValue = request?.Cookies[CookieRequestCultureProvider.DefaultCookieName];

        if (cookieValue == null)
            return null;

        var requestCulture = CookieRequestCultureProvider.ParseCookieValue(cookieValue);
        return requestCulture?.Cultures.FirstOrDefault().Value;
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    public IStringLocalizer WithCulture(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        return this;
    }
}
