using System.Collections.Concurrent;

namespace API.Services;

public class LocalizationFileProvider
{
    private readonly ConcurrentDictionary<string, string> _cachedLocaleFiles = [];
    private readonly string _localesPath;
    private readonly string _fallbackLocalePath;
    private const string FallbackLocale = "en";

    public LocalizationFileProvider(IWebHostEnvironment env)
    {
        _localesPath = Path.Combine(env.ContentRootPath, "Resources", "Locales");
        _fallbackLocalePath = Path.Combine(_localesPath, $"{FallbackLocale}.json");
    }

    public async ValueTask<string> GetLocalizationAsync(string locale)
    {
        if (_cachedLocaleFiles.TryGetValue(locale, out var cachedContent))
        {
            return cachedContent;
        }

        var filePath = GetFilePath(locale, out var isFallback);

        if (isFallback == false)
        {
            var fileContent = await File.ReadAllTextAsync(filePath);
            _cachedLocaleFiles.TryAdd(locale, fileContent);
            return fileContent;
        }

        if (_cachedLocaleFiles.TryGetValue(FallbackLocale, out var cachedFallbackContent))
        {
            return cachedFallbackContent;
        }

        var fallbackFileContent = await File.ReadAllTextAsync(filePath);
        _cachedLocaleFiles.TryAdd(FallbackLocale, fallbackFileContent);
        return fallbackFileContent;
    }

    private string GetFilePath(string locale, out bool isFallback)
    {
        isFallback = false;
        var filePath = Path.Combine(_localesPath, $"{locale}.json");

        if (File.Exists(filePath) == true)
        {
            return filePath;
        }

        if (File.Exists(_fallbackLocalePath) == false)
        {
            throw new FileNotFoundException(
                $"Localization file for '{FallbackLocale}' could not be found by path '{_fallbackLocalePath}'."
            );
        }

        isFallback = true;
        return _fallbackLocalePath;
    }
}
