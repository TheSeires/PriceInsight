using API.Extensions;
using API.Models.Application;
using API.Models.Application.Dto.Responses;
using API.Models.Database;
using API.Models.Dto;
using API.Models.Dto.Requests;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;

namespace API.Endpoints.V1;

public static partial class CategoryEndpoints
{
    public static void MapCategoryEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/categories");
        group.MapGet("/", GetCategoryList);
        group.MapGet("/{id}", GetCategoryById).WithName(nameof(GetCategoryById));
        group.MapPost("/", CreateCategory).Validate<CreateCategoryRequest>();
        group.MapPut("/", UpdateCategory).Validate<UpdateCategoryRequest>();
    }

    private static async Task<IResult> GetCategoryList(
        ICategoryService categoryService,
        ICategoryMappingService categoryMappingService,
        [FromQuery] string? excludeMappedCategory,
        QueryFilters filters,
        CancellationToken cancellationToken = default)
    {
        var aggregateOperator = AggregateOperator<Category>.Create();

        if (excludeMappedCategory.IsNullOrWhiteSpace() == false)
        {
            var mappedCategories = await categoryMappingService.FindByAsync(
                m => m.SourceCategory == excludeMappedCategory, cancellationToken);
            var mappedCategoryIds = mappedCategories.Select(m => m.Id);
            aggregateOperator = aggregateOperator.Where(x => mappedCategoryIds.Contains(x.Id) == false);
        }

        var filtersResult = aggregateOperator.MaybeAddQueryFilters(filters);
        if (filtersResult.PaginationFilterApplied)
        {
            var paginatedCategories = await categoryService.FindByAsync(aggregateOperator, cancellationToken);
            var totalItems = await categoryService.CountAsync(cancellationToken);
            return Results.Ok(new PaginatedItemsResponse<Category>(paginatedCategories, totalItems, filtersResult.NormalizedLimit));
        }

        var categories = await categoryService.FindByAsync(aggregateOperator, cancellationToken);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryById(
        [FromRoute] string id,
        ICachedCategoryService cachedCategoryService,
        CancellationToken cancellationToken)
    {
        var category = await cachedCategoryService.FindOneByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(category);
    }

    private static async Task<IResult> CreateCategory(
        [FromBody] CreateCategoryRequest createCategory,
        ICachedCategoryService cachedCategoryService,
        IStringLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateCategoryParentFormat(createCategory, localizer);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var categoryExists = await cachedCategoryService.FindOneByAsync(x =>
            x.Name == createCategory.Name, cancellationToken);
        if (categoryExists is not null)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["models.category.validation.alreadyExists"]));
        }

        var newCategory = await cachedCategoryService.CreateAsync(
            new Category
            {
                Name = createCategory.Name,
                ParentId = createCategory.ParentId.IfNullOrEmpty(ObjectId.Empty.ToString()),
                NameTranslationKey = createCategory.NameTranslationKey,
                ShortId = createCategory.ShortId,
            }, cancellationToken);

        return Results.CreatedAtRoute(nameof(GetCategoryById), newCategory);
    }

    private static async Task<IResult> UpdateCategory(
        [FromBody] UpdateCategoryRequest updateCategory,
        ICachedCategoryService cachedCategoryService,
        IStringLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateCategoryParentFormat(updateCategory, localizer);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var updateResult = await cachedCategoryService.UpdateAsync(updateCategory.Id,
            new Category
            {
                Id = updateCategory.Id,
                Name = updateCategory.Name,
                ParentId = updateCategory.ParentId.IfNullOrEmpty(ObjectId.Empty.ToString()),
                NameTranslationKey = updateCategory.NameTranslationKey,
                ShortId = updateCategory.ShortId
            }, cancellationToken);

        if (updateResult.ModifiedCount == 0)
        {
            return Results.InternalServerError();
        }

        return Results.Ok(updateResult);
    }

    private static IResult? ValidateCategoryParentFormat(
        CreateCategoryRequest createCategory,
        IStringLocalizer localizer)
    {
        if (createCategory.ParentId.IsNullOrWhiteSpace() == false
            && ObjectId.TryParse(createCategory.ParentId, out _) == false)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["models.category.validation.invalidParentIdFormat"]));
        }

        return null;
    }
}
