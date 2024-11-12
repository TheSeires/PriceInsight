using API.Models.Application;
using API.Models.Database;
using API.Models.Dto;
using API.Services.Interfaces;

namespace API.Endpoints.V1;

public static partial class CategorizationIssueEndpoint
{
    public static void MapCategorizationIssueEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/categorization-issues");
        group.MapGet("/", GetCategorizationIssues);
    }

    private static async Task<IResult> GetCategorizationIssues(
        ICategorizationIssueService categorizationIssueService,
        QueryFilters filters,
        CancellationToken cancellationToken)
    {
        var aggregateOperator = new AggregateOperator<CategorizationIssue>()
            .Lookup<Market, CategorizationIssueDto>(
                DbCollectionNames.MarketCollectionName,
                c => c.MarketId,
                m => m.Id,
                dto => dto.Market)
            .Unwind(dto => dto.Market);

        var filtersResult = aggregateOperator.MaybeAddQueryFilters(filters);

        if (filtersResult.PaginationFilterApplied)
        {
            var paginatedIssues = await categorizationIssueService.FindByAsync(aggregateOperator, cancellationToken);
            var totalItems = await categorizationIssueService.CountAsync(cancellationToken);
            return Results.Ok(new PaginatedItemsResponse<CategorizationIssueDto>(
                paginatedIssues, totalItems, filtersResult.NormalizedLimit));
        }

        var issues = await categorizationIssueService.FindByAsync(aggregateOperator, cancellationToken);
        return Results.Ok(issues);
    }
}
