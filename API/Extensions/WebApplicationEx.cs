namespace API.Extensions;

public static class WebApplicationEx
{
    public static void UseSpaMiddleware(this WebApplication app, PathString spaPath)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/" ||
                (context.Response.StatusCode == 404 &&
                 !Path.HasExtension(context.Request.Path.Value) &&
                 !(context.Request.Path.Value?.StartsWith("/api/") ?? false)))
            {
                context.Request.Path = spaPath;
            }

            await next();
        });
    }
}