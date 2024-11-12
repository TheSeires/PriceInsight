using System.Linq.Expressions;
using API.Extensions;
using API.Models.Application.Interfaces;
using API.Models.Dto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace API.Models.Application;

public class AggregateOperator<TEntity> : BaseAggregateOperator, IAggregateOperator<TEntity>
{
    private readonly List<BsonDocument> _pipeline = [];

    public AggregateOperator()
    {
    }

    public static IAggregateOperator<TEntity> Create() => new AggregateOperator<TEntity>();

    private AggregateOperator<TNewResultEntity> CloneToNewInstance<TNewResultEntity>()
    {
        var newOperator = new AggregateOperator<TNewResultEntity>();
        newOperator._pipeline.AddRange(_pipeline);
        return newOperator;
    }

    public List<BsonDocument> GetPipeline()
    {
        return _pipeline;
    }

    private string GetMemberName(Expression expression)
    {
        if (expression is LambdaExpression lambda)
        {
            MemberExpression? memberExpression;

            switch (lambda.Body)
            {
                case UnaryExpression unaryExpression:
                    memberExpression = unaryExpression.Operand as MemberExpression;
                    break;
                case ConstantExpression { Value: string str }:
                    return str;
                default:
                    memberExpression = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpression != null)
            {
                if (memberExpression.Member.Name.ToLower() == "id")
                {
                    return "_id";
                }

                return UseCamelCaseFieldNames
                    ? memberExpression.Member.Name.Uncapitilize()
                    : memberExpression.Member.Name;
            }
        }

        throw new ArgumentException("Invalid expression. Expected a property expression.", nameof(expression));
    }

    public IAggregateOperator<TResultEntity> Lookup<TForeignEntity, TResultEntity>(
        string foreignCollectionName,
        Expression<Func<TEntity, object>> localFieldExpression,
        Expression<Func<TForeignEntity, object>> foreignFieldExpression,
        Expression<Func<TResultEntity, object?>> asFieldExpression)
    {
        _pipeline.Add(
            new BsonDocument(
                "$lookup",
                new BsonDocument
                {
                    { "from", foreignCollectionName },
                    { "localField", GetMemberName(localFieldExpression) },
                    { "foreignField", GetMemberName(foreignFieldExpression) },
                    { "as", GetMemberName(asFieldExpression) },
                }
            )
        );

        return CloneToNewInstance<TResultEntity>();
    }

    public IAggregateOperator<TResultEntity> Lookup<TForeignEntity, TResultEntity>(
        string foreignCollectionName,
        Expression<Func<TEntity, object>> localFieldExpression,
        Expression<Func<TForeignEntity, object>> foreignFieldExpression,
        string @as)
    {
        _pipeline.Add(
            new BsonDocument(
                "$lookup",
                new BsonDocument
                {
                    { "from", foreignCollectionName },
                    { "localField", GetMemberName(localFieldExpression) },
                    { "foreignField", GetMemberName(foreignFieldExpression) },
                    { "as", UseCamelCaseFieldNames ? @as.Uncapitilize() : @as },
                })
        );

        return CloneToNewInstance<TResultEntity>();
    }

    public QueryFiltersResult MaybeAddQueryFilters(QueryFilters? queryFiltering, int maxLimit = 20)
    {
        if (queryFiltering is null)
            return QueryFiltersResult.Empty;

        return new QueryFiltersResult
        {
            MatchFiltersApplied = MaybeAddMatchFilters(queryFiltering),
            SortFilterApplied = MaybeAddSortFilter(queryFiltering),
            PaginationFilterApplied = MaybeAddPaginationFilter(queryFiltering, maxLimit, out var normalizedLimit),
            NormalizedLimit = normalizedLimit
        };
    }

    private bool MaybeAddMatchFilters(QueryFilters queryFiltering)
    {
        if (queryFiltering.MatchFilters is null)
            return false;

        var filterBuilder = Builders<TEntity>.Filter;
        var filtersList = new List<FilterDefinition<TEntity>>();

        foreach (var (rawFieldName, valueExpressions) in queryFiltering.MatchFilters)
        {
            var fieldName = rawFieldName == "id" ? "_id" : rawFieldName;

            foreach (var valueExpression in valueExpressions)
            {
                var index = valueExpression.IndexOf(':');
                if (index != -1)
                {
                    var value = valueExpression[(index + 1)..];
                    var operatorOption = valueExpression[..index];

                    switch (operatorOption)
                    {
                        case "exists":
                            if (bool.TryParse(value, out var boolValue))
                            {
                                filtersList.Add(filterBuilder.Exists(fieldName, boolValue));
                            }

                            break;
                        case "regex":
                            filtersList.Add(filterBuilder.Regex(fieldName, value));
                            break;
                        case "lt":
                            filtersList.Add(filterBuilder.Lt(fieldName, value));
                            break;
                        case "gt":
                            filtersList.Add(filterBuilder.Gt(fieldName, value));
                            break;
                        case "lte":
                            filtersList.Add(filterBuilder.Lte(fieldName, value));
                            break;
                        case "gte":
                            filtersList.Add(filterBuilder.Gte(fieldName, value));
                            break;
                        case "eq":
                            filtersList.Add(filterBuilder.Eq(fieldName, ParseMatchValueToObject(value)));
                            break;
                        case "ne":
                            filtersList.Add(filterBuilder.Ne(fieldName, ParseMatchValueToObject(value)));
                            break;
                        case "in":
                            var arrayValues = ParseMatchValueToArray(value);
                            filtersList.Add(filterBuilder.In(fieldName, arrayValues));
                            break;
                    }
                }
            }
        }

        var combinedFilter = filtersList.Count > 0 ? filterBuilder.And(filtersList) : filterBuilder.Empty;
        if (combinedFilter == filterBuilder.Empty)
            return false;

        _pipeline.Add(new("$match",
            combinedFilter.Render(
                BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(),
                BsonSerializer.SerializerRegistry)));
        return true;
    }

    private List<object> ParseMatchValueToArray(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith('[') || !value.EndsWith(']'))
            return [];

        value = value[1..^1];

        var result = new List<object>();
        foreach (var item in value.Split(','))
        {
            var trimmedItem = item.Trim();
            result.Add(ParseMatchValueToObject(trimmedItem));
        }

        return result;
    }

    private object ParseMatchValueToObject(string value)
    {
        if (ObjectId.TryParse(value, out var objectId))
        {
            return objectId;
        }

        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        if (int.TryParse(value, out var integer))
        {
            return integer;
        }

        return value;
    }

    private bool MaybeAddSortFilter(QueryFilters queryFiltering)
    {
        if (queryFiltering.SortBy.IsNullOrWhiteSpace())
            return false;

        var sortBuilder = Builders<TEntity>.Sort;
        var sortBy = queryFiltering.SortBy;
        var sortDirection = queryFiltering.SortDirection;
        var sortDefinition = sortDirection == SortDirection.Ascending
            ? sortBuilder.Ascending(sortBy)
            : sortBuilder.Descending(sortBy);

        _pipeline.Add(new("$sort",
            sortDefinition.Render(
                BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(),
                BsonSerializer.SerializerRegistry)));
        return true;
    }

    private bool MaybeAddPaginationFilter(QueryFilters queryFiltering, int maxLimit, out int normalizedLimit)
    {
        normalizedLimit = default;
        if (queryFiltering.Page != null)
        {
            var (skip, take) = PaginationEx.GetNormalizedPaginationFilters(
                queryFiltering.Page.Value, queryFiltering.Limit, maxLimit);

            normalizedLimit = take;
            Skip(skip);
            Take(take);
            return true;
        }

        return false;
    }

    public IAggregateOperator<TEntity> SortAscending(Expression<Func<TEntity, object>> field)
    {
        var sortAsc = Builders<TEntity>.Sort.Ascending(field)
            .Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(),
                BsonSerializer.SerializerRegistry);

        _pipeline.Add(new("$sort", sortAsc));
        return this;
    }

    public IAggregateOperator<TEntity> SortDescending(Expression<Func<TEntity, object>> field)
    {
        var sortDesc = Builders<TEntity>.Sort.Descending(field)
            .Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(),
                BsonSerializer.SerializerRegistry);

        _pipeline.Add(new("$sort", sortDesc));
        return this;
    }

    public IAggregateOperator<TEntity> Unwind(Expression<Func<TEntity, object?>> fieldExpression)
    {
        _pipeline.Add(new("$unwind", $"${GetMemberName(fieldExpression)}"));
        return this;
    }

    public IAggregateOperator<TEntity> Unwind(string variable)
    {
        _pipeline.Add(new("$unwind", $"${variable.Uncapitilize()}"));
        return this;
    }

    public IAggregateOperator<TEntity> Where(Expression<Func<TEntity, bool>> expression)
    {
        var filter = Builders<TEntity>
            .Filter.Where(expression)
            .Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(),
                BsonSerializer.SerializerRegistry);

        _pipeline.Add(new("$match", filter));
        return this;
    }

    public IAggregateOperator<TResultEntity> Where<TResultEntity>(
        Expression<Func<TResultEntity, bool>> expression
    )
    {
        var filter = Builders<TResultEntity>
            .Filter.Where(expression)
            .Render(BsonSerializer.SerializerRegistry.GetSerializer<TResultEntity>(),
                BsonSerializer.SerializerRegistry);

        _pipeline.Add(new("$match", filter));
        return CloneToNewInstance<TResultEntity>();
    }

    public IAggregateOperator<TEntity> AddStage(BsonDocument bsonElements)
    {
        _pipeline.Add(bsonElements);
        return this;
    }

    public IAggregateOperator<TEntity> Skip(long skip = 0)
    {
        if (skip > 0)
        {
            _pipeline.Add(new("$skip", skip));
        }

        return this;
    }

    public IAggregateOperator<TEntity> Take(long take = 0)
    {
        if (take > 0)
        {
            _pipeline.Add(new("$limit", take));
        }

        return this;
    }

    public IAggregateOperator<TEntity> MergeRoot(string otherFieldsName = "otherFields",
        params MergeParameter[] additionalFields)
    {
        if (additionalFields.Length == 0)
            return this;

        var groupFields = new BsonDocument
        {
            { "_id", "$_id" },
            { otherFieldsName, new BsonDocument("$first", "$$ROOT") },
        };
        foreach (var (fieldName, isArray) in additionalFields)
        {
            groupFields.Add(fieldName, new BsonDocument(isArray ? "$push" : "$first", $"${fieldName}"));
        }

        var mergeObjectFields = new BsonDocument();
        foreach (var (fieldName, _) in additionalFields)
        {
            mergeObjectFields.Add(fieldName, $"${fieldName}");
        }

        _pipeline.Add(new BsonDocument("$group", groupFields));

        _pipeline.Add(new BsonDocument("$replaceRoot", new BsonDocument
        {
            {
                "newRoot", new BsonDocument
                {
                    {
                        "$mergeObjects", new BsonArray
                        {
                            $"${otherFieldsName}",
                            mergeObjectFields
                        }
                    }
                }
            }
        }));

        return this;
    }

    public IAggregateOperator<TEntity> Regex(Expression<Func<TEntity, object>> localFieldExpression, string searchTerm,
        string options = "i")
    {
        _pipeline.Add(new BsonDocument("$match", new BsonDocument(GetMemberName(localFieldExpression),
            new BsonDocument
            {
                { "$regex", searchTerm },
                { "$options", options }
            }
        )));
        return this;
    }
}