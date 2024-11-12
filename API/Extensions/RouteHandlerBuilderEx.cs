using API.Models.Application.Dto.Responses;
using Microsoft.Extensions.Localization;

namespace API.Extensions;

public static class RouteHandlerBuilderEx
{
    public static RouteHandlerBuilder Validate<T>(
        this RouteHandlerBuilder builder,
        bool firstErrorOnly = true
    )
    {
        builder.AddEndpointFilter(
            async (invocationContext, next) =>
            {
                var httpContext = invocationContext.HttpContext;
                var localizer = httpContext.RequestServices.GetRequiredService<IStringLocalizer>();

                var argument = invocationContext.Arguments.OfType<T>().FirstOrDefault();
                var response = argument?.DataAnnotationsValidate();

                if (response is not null && !response.Value.IsValid)
                {
                    if (firstErrorOnly)
                    {
                        var localizationKey = response.Value.Results.FirstOrDefault()?.ErrorMessage;
                        if (localizationKey.IsNullOrWhiteSpace() == false)
                        {
                            return Results.BadRequest(new ErrorResponse(localizer[localizationKey]));
                        }
                    }
                    else
                    {
                        var joinedLocalizedMessages = string.Join(". ", response.Value.Results
                            .Where(x => x.ErrorMessage.IsNullOrWhiteSpace() == false)
                            .Select(x => localizer[x.ErrorMessage!]));
                        if (joinedLocalizedMessages.IsNullOrWhiteSpace() == false)
                        {
                            return Results.BadRequest(new ErrorResponse(joinedLocalizedMessages));
                        }
                    }

                    return Results.BadRequest();
                }

                return await next(invocationContext);
            }
        );

        return builder;
    }
}
