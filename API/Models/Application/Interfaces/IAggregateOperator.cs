using System.Linq.Expressions;
using API.Models.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Models.Application.Interfaces;

public interface IAggregateOperator<TEntity>
{
    List<BsonDocument> GetPipeline();

    /// <returns>
    /// A new instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TResultEntity> Lookup<TForeignEntity, TResultEntity>(
        string foreignCollectionName,
        Expression<Func<TEntity, object>> localFieldExpression,
        Expression<Func<TForeignEntity, object>> foreignFieldExpression,
        Expression<Func<TResultEntity, object?>> asFieldExpression
    );

    /// <returns>
    /// A new instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TResultEntity> Lookup<TForeignEntity, TResultEntity>(
        string foreignCollectionName,
        Expression<Func<TEntity, object>> localFieldExpression,
        Expression<Func<TForeignEntity, object>> foreignFieldExpression,
        string @as
    );

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> Unwind(Expression<Func<TEntity, object?>> fieldExpression);

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> Unwind(string variable);

    QueryFiltersResult MaybeAddQueryFilters(QueryFilters? queryFiltering, int maxLimit = 20);

    /// <summary>
    /// Sorts the documents in ascending order based on the specified field.
    /// </summary>
    /// <param name="field">The field to sort the documents by.</param>
    /// <returns>The same instance of <see cref="AggregateOperator{TEntity}"/> with the applied sort operation.</returns>
    IAggregateOperator<TEntity> SortAscending(Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Sorts the documents in descending order based on the specified field.
    /// </summary>
    /// <param name="field">The field to sort the documents by.</param>
    /// <returns>The same instance of <see cref="AggregateOperator{TEntity}"/> with the applied sort operation.</returns>
    IAggregateOperator<TEntity> SortDescending(Expression<Func<TEntity, object>> field);

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> Where(Expression<Func<TEntity, bool>> expression);

    /// <returns>
    /// A new instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TResultEntity> Where<TResultEntity>(
        Expression<Func<TResultEntity, bool>> expression
    );

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> AddStage(BsonDocument bsonElements);

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> Skip(long skip = 0);

    /// <returns>
    /// The same instance of <see cref="AggregateOperator{TEntity}"/>
    /// </returns>
    IAggregateOperator<TEntity> Take(long take = 0);

    IAggregateOperator<TEntity> MergeRoot(string otherFieldsName = "otherFields",
        params MergeParameter[] additionalFields);

    IAggregateOperator<TEntity> Regex(Expression<Func<TEntity, object>> localFieldExpression, string searchTerm,
        string options = "i");
}