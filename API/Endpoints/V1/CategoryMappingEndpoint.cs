using API.Extensions;
using API.Models.Application;
using API.Models.Application.Dto.Requests;
using API.Models.Application.Dto.Responses;
using API.Models.Mappers;
using API.Services.Background;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace API.Endpoints.V1;

public static partial class CategoryMappingEndpoint
{
    public static void MapCategoryMappingEndpointsV1(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup($"/api/{ApiVersion.V1}/category-mappings");
        group.MapPost("/", CreateCategoryMapping).Validate<CreateCategoryMappingRequest>();
    }

    [Authorize(Roles = UserRoles.Admin)]
    private static async Task<IResult> CreateCategoryMapping(
        [FromBody] CreateCategoryMappingRequest createCatMapRequest,
        IStringLocalizer localizer,
        ICategoryService categoryService,
        ICategoryMappingService categoryMappingService,
        IMarketService marketService,
        CategoryUpdaterService categoryUpdaterService)
    {
        var marketExists = await marketService.FindOneByIdAsync(createCatMapRequest.SourceMarketId) is not null;
        if (marketExists == false)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["models.categoryMapping.validation.invalidMarketId"]));
        }

        var categoryExists = await categoryService.FindOneByIdAsync(createCatMapRequest.TargetCategoryId) is not null;
        if (categoryExists == false)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["models.categoryMapping.validation.invalidTargetCategoryId"]));
        }

        var mappingExists = await categoryMappingService.FindOneByAsync(x =>
            x.SourceMarketId == createCatMapRequest.SourceMarketId
            && x.TargetCategoryId == createCatMapRequest.TargetCategoryId
            && x.SourceCategory == createCatMapRequest.SourceCategory) is not null;
        if (mappingExists)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["models.categoryMapping.validation.alreadyExists"]));
        }

        var categoryMapping = await categoryMappingService.CreateAsync(createCatMapRequest.MapToCategoryMapping());
        categoryUpdaterService.SkipDelay();
        return Results.Ok(categoryMapping);
    }
}