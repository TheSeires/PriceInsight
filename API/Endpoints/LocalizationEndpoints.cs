using API.Services;
using Microsoft.AspNetCore.Localization;

namespace API.Endpoints;

public static class LocalizationEndpoints
{
    public static void MapLocalizationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/localization");
        group.MapGet("/{locale}", GetLocalization);
        group.MapGet("/set-culture", SetCultureCookie);
    }

    private static async Task<IResult> GetLocalization(
        string locale,
        bool changeLanguage,
        LocalizationFileProvider localizationFileProvider,
        ILogger<Program> logger,
        HttpContext context)
    {
        try
        {
            EnsureCultureCookieIsSet(context, locale, changeLanguage);
            var jsonContent = await localizationFileProvider.GetLocalizationAsync(locale);
            return Results.Content(jsonContent, "application/json");
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return Results.NotFound();
        }
    }

    private static IResult SetCultureCookie(string locale, HttpContext context)
    {
        EnsureCultureCookieIsSet(context, locale, true);
        return Results.Ok();
    }

    private static void EnsureCultureCookieIsSet(
        HttpContext context,
        string locale,
        bool changingLanguage)
    {
        var cultureCookieName = CookieRequestCultureProvider.DefaultCookieName;
        var isCultureCookieSet = context.Request.Cookies.ContainsKey(cultureCookieName);

        if (isCultureCookieSet && !changingLanguage) return;

        var requestCulture = new RequestCulture(locale, locale);
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);
        var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) };
        context.Response.Cookies.Append(cultureCookieName, cookieValue, cookieOptions);
    }
}