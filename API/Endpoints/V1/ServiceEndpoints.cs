using API.Models.Application;
using API.Models.Application.Dto.Responses;
using API.Services.Background;
using Microsoft.AspNetCore.Authorization;

namespace API.Endpoints.V1;

public static partial class ServiceEndpoints
{
    public static void MapServiceEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/services");
        group.MapGet("/force-start-crawling", ForceStartCrawlingAsync);
    }

    [Authorize(Roles = UserRoles.Admin)]
    private static IResult ForceStartCrawlingAsync(MarketCrawlersStarter crawlersStarter,
        bool parseProductPageInfo = false)
    {
        var emptyResult = crawlersStarter.SkipDelay(parseProductPageInfo);

        return emptyResult.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(new ErrorResponse(emptyResult.Error));
    }
}