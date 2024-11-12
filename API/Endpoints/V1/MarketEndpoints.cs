using API.Models.Application;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints.V1;

public static partial class MarketEndpoints
{
    public static void MapMarketEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/markets");
        group.MapGet("/", GetMarkets);
    }

    private static async Task<IResult> GetMarkets(ICachedMarketService marketService,
        CancellationToken cancellationToken)
    {
        var result = await marketService.FindByAsync(null, cancellationToken);
        return Results.Ok(result);
    }
}