using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using API.Extensions;
using API.Models.Application;
using API.Models.Application.Dto.Responses;
using API.Models.Application.Interfaces;
using API.Models.Database;
using API.Models.Dto;
using API.Models.Dto.Requests;
using API.Services.Interfaces;
using Dumpify;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Endpoints.V1;

public static partial class ProductEndpoints
{
    public static void MapProductEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/products");
        group.MapGet("/{id}", GetProductById);
        group.MapGet("/", GetProducts);
        group.MapGet("/catalog", GetCatalogProducts);
    }

    private static async Task<IResult> GetProductById(string id, IProductService productService,
        IMarketService marketService,
        CancellationToken cancellationToken)
    {
        if (ObjectId.TryParse(id, out _) == false)
        {
            return Results.BadRequest(new ErrorResponse("common.validation.query.invalidIdFormat"));
        }

        var aggregator = IncludeCategory(new AggregateOperator<ProductDto>());
        aggregator = IncludePrices(aggregator);

        var result = await productService.FindOneByIdAsync(id, aggregator, cancellationToken);
        if (result is null)
        {
            return Results.NotFound();
        }

        await result.PriceEntries.IncludeMarketsAsync(marketService, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProducts(ICachedProductService cachedProductService, QueryFilters filters,
        CancellationToken cancellationToken)
    {
        var aggregateOperator = IncludeCategory(new AggregateOperator<ProductDto>());
        aggregateOperator = IncludePrices(aggregateOperator);

        var filtersResult = aggregateOperator.MaybeAddQueryFilters(filters);
        if (filtersResult.PaginationFilterApplied)
        {
            var paginatedProducts = await cachedProductService.FindByAsync(aggregateOperator, cancellationToken);
            var total = await cachedProductService.CountAsync(cancellationToken);

            return Results.Ok(
                new PaginatedItemsResponse<ProductDto>(paginatedProducts, total, filtersResult.NormalizedLimit));
        }

        var products = await cachedProductService.FindByAsync(aggregateOperator, cancellationToken);
        return Results.Ok(products);
    }

    private static async Task<IResult> GetCatalogProducts([AsParameters] GetCatalogProductsRequest catalogRequest,
        QueryFilters queryFilters, ICachedProductService cachedProductService,
        ICachedPriceEntryService cachedPriceEntryService, CancellationToken cancellationToken)
    {
        var countFilterDef = Builders<Product>.Filter.Where(x => x.CategoryId != ObjectId.Empty.ToString());
        var aggregateOperator = AggregateOperator<ProductDto>.Create();
        if (catalogRequest.CategoryId.IsNullOrWhiteSpace() == false)
        {
            countFilterDef = Builders<Product>.Filter.And(countFilterDef,
                Builders<Product>.Filter.Where(x => x.CategoryId == catalogRequest.CategoryId));
            aggregateOperator.Where(x => x.CategoryId == catalogRequest.CategoryId);
        }

        if (TryAddPriceEntryFilters(catalogRequest, out var priceEntryFilters))
        {
            var priceEntries =
                await cachedPriceEntryService.FindByAsync(priceEntryFilters, cancellationToken);
            var productIds = priceEntries.Select(x => x.ProductId).ToArray();
            countFilterDef = Builders<Product>.Filter.And(countFilterDef,
                Builders<Product>.Filter.Where(x => productIds.Contains(x.Id)));
            aggregateOperator = aggregateOperator.Where(x => productIds.Contains(x.Id));
        }

        if (catalogRequest.Search.IsNullOrWhiteSpace() == false)
        {
            countFilterDef = Builders<Product>.Filter.And(countFilterDef,
                Builders<Product>.Filter.Regex(x => x.Name, new BsonRegularExpression(catalogRequest.Search, "i")));
            aggregateOperator.Regex(x => x.Name, catalogRequest.Search);
        }

        var filtersResult = aggregateOperator.MaybeAddQueryFilters(queryFilters, 50);
        aggregateOperator = IncludeCategory(aggregateOperator);
        aggregateOperator = IncludePrices(aggregateOperator);

        var products = await cachedProductService.FindByAsync(aggregateOperator, cancellationToken);
        var total = await cachedProductService.CountAsync(countFilterDef, cancellationToken);
        return Results.Ok(new PaginatedItemsResponse<ProductDto>(products, total, filtersResult.NormalizedLimit));
    }

    private static bool TryAddPriceEntryFilters(GetCatalogProductsRequest catalogRequest,
        [NotNullWhen(true)] out Expression<Func<PriceEntry, bool>>? expression)
    {
        var filterWasApplied = false;
        expression = null;

        if (catalogRequest.Discounted)
        {
            expression = x => x.DiscountedPrice != null;
            filterWasApplied = true;
        }

        if (catalogRequest.MinPrice.HasValue)
        {
            var minPrice = catalogRequest.MinPrice.Value;
            expression = expression?.CombineWith(
                             x => x.Price >= minPrice || x.DiscountedPrice >= minPrice,
                             Expression.AndAlso)
                         ?? (x => x.Price >= minPrice || x.DiscountedPrice >= minPrice);
            filterWasApplied = true;
        }

        if (catalogRequest.MaxPrice.HasValue)
        {
            var maxPrice = catalogRequest.MaxPrice.Value;
            expression = expression?.CombineWith(
                             x => x.Price <= maxPrice || x.DiscountedPrice <= maxPrice,
                             Expression.AndAlso)
                         ?? (x => x.Price <= maxPrice || x.DiscountedPrice <= maxPrice);
            filterWasApplied = true;
        }

        return filterWasApplied;
    }

    private static IAggregateOperator<ProductDto> IncludeCategory(IAggregateOperator<ProductDto> aggregator) =>
        aggregator.Lookup<Category, ProductDto>(
                DbCollectionNames.CategoryCollectionName,
                p => p.CategoryId,
                c => c.Id,
                dto => dto.Category)
            .Unwind(dto => dto.Category);

    private static IAggregateOperator<ProductDto> IncludePrices(IAggregateOperator<ProductDto> aggregator) =>
        aggregator.Lookup<PriceEntry, ProductDto>(
            DbCollectionNames.PriceEntryCollectionName,
            p => p.Id,
            pe => pe.ProductId,
            dto => dto.PriceEntries);
}