using API.Models.Application;
using API.Models.Dto;
using API.Services.Interfaces;

namespace API.Endpoints.V1;

public static partial class PriceEntryEndpoints
{
    public static void MapPriceEntryEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/price-entries");
        group.MapGet("/", GetProducts);
    }

    private static async Task<IResult> GetProducts(ICachedPriceEntryService cachedPriceEntryService,
        QueryFilters filters, CancellationToken cancellationToken)
    {
        var aggregateOperator = new AggregateOperator<PriceEntryDto>();

        var filtersResult = aggregateOperator.MaybeAddQueryFilters(filters);
        if (filtersResult.PaginationFilterApplied)
        {
            var paginatedPriceEntries = await cachedPriceEntryService.FindByAsync(aggregateOperator, cancellationToken);
            var total = await cachedPriceEntryService.CountAsync(cancellationToken);

            return Results.Ok(new PaginatedItemsResponse<PriceEntryDto>(
                paginatedPriceEntries, total, filtersResult.NormalizedLimit));
        }

        var priceEntries = await cachedPriceEntryService.FindByAsync(aggregateOperator, cancellationToken);
        return Results.Ok(priceEntries);
    }
}