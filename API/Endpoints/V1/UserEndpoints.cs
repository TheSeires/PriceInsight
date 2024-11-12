using System.Security.Claims;
using API.Models.Application;
using API.Models.Database;
using API.Models.Dto;
using API.Models.Dto.Requests;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Endpoints.V1;

public static partial class UserEndpoints
{
    public static void MapUserEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/users");

        group.MapGet("/{userId:guid}/product-followings", GetProductFollowings);
        group.MapPost("/{userId:guid}/product-followings", CreateProductFollowing);
        group.MapDelete("/{userId:guid}/product-followings", DeleteProductFollowing);

        group.MapGet("/{userId:guid}/followed-products", GetFollowedProducts);
    }

    [Authorize]
    private static async Task<IResult> GetProductFollowings(Guid userId,
        IProductFollowingService productFollowingService,
        ClaimsPrincipal claimsPrincipal, UserManager<User> userManager, CancellationToken cancellationToken)
    {
        var userIdStr = userId.ToString();
        if (claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) != userIdStr)
        {
            return Results.BadRequest();
        }

        var prodFollowings = await productFollowingService.FindByAsync(x => x.UserId == userIdStr, cancellationToken);
        return Results.Ok(prodFollowings);
    }

    [Authorize]
    private static async Task<IResult> GetFollowedProducts(Guid userId, ClaimsPrincipal claimsPrincipal,
        QueryFilters filters, ICachedProductService productService,
        ICachedProductFollowingService productFollowingService, CancellationToken cancellationToken)
    {
        var userIdStr = userId.ToString();
        if (claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) != userIdStr)
        {
            return Results.BadRequest();
        }

        var productFollowingAggregator = new AggregateOperator<ProductFollowing>()
            .Where(x => x.UserId == userIdStr);
        var filtersResult = productFollowingAggregator.MaybeAddQueryFilters(filters);

        var productFollowings =
            await productFollowingService.FindByAsync(productFollowingAggregator, cancellationToken);
        if (productFollowings.Count == 0)
            return Results.NoContent();

        var followingProductIds = productFollowings.Select(x => x.ProductId);
        var productAggregator = new AggregateOperator<Product>()
            .Where(x => followingProductIds.Contains(x.Id))
            .Lookup<PriceEntry, ProductDto>(
                DbCollectionNames.PriceEntryCollectionName,
                p => p.Id,
                pe => pe.ProductId,
                dto => dto.PriceEntries);

        var products = await productService.FindByAsync(productAggregator, cancellationToken);

        if (filtersResult.PaginationFilterApplied)
        {
            var countFilterDef = Builders<ProductFollowing>.Filter.Where(x => x.UserId == userIdStr);
            var totalFollowings = await productFollowingService.CountAsync(countFilterDef, cancellationToken);

            return Results.Ok(
                new PaginatedItemsResponse<ProductDto>(products, totalFollowings, filtersResult.NormalizedLimit));
        }

        return Results.Ok(products);
    }

    [Authorize]
    private static async Task<IResult> CreateProductFollowing(Guid userId, ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateProductFollowingRequest createRequest, IProductFollowingService productFollowingService,
        CancellationToken cancellationToken)
    {
        var userIdStr = userId.ToString();
        if (claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) != userIdStr)
        {
            return Results.BadRequest();
        }

        if (ObjectId.TryParse(createRequest.ProductId, out _) == false)
        {
            return Results.BadRequest();
        }

        var productFollowing = new ProductFollowing
        {
            UserId = userIdStr,
            ProductId = createRequest.ProductId,
            Created = DateTime.UtcNow
        };

        var result = await productFollowingService.CreateAsync(productFollowing, cancellationToken);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> DeleteProductFollowing(Guid userId, ClaimsPrincipal claimsPrincipal,
        [FromBody] DeleteProductFollowingRequest deleteRequest, IProductFollowingService productFollowingService,
        CancellationToken cancellationToken)
    {
        var userIdStr = userId.ToString();
        if (claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) != userIdStr)
        {
            return Results.BadRequest();
        }

        if (ObjectId.TryParse(deleteRequest.ProductId, out _) == false)
        {
            return Results.BadRequest();
        }

        var productFollowing = await productFollowingService.FindOneByAsync(
            x => x.UserId == userIdStr && x.ProductId == deleteRequest.ProductId, cancellationToken);
        if (productFollowing is null)
        {
            return Results.NoContent();
        }

        var result = await productFollowingService.DeleteAsync(productFollowing.Id, cancellationToken);
        return result.DeletedCount != 0 ? Results.Ok() : Results.InternalServerError();
    }
}