using System.Linq.Expressions;
using API.Models.Application.Interfaces;
using API.Models.Database.Interfaces;
using MongoDB.Driver;

namespace API.Services.Interfaces;

public interface IRepositoryService<TEntity, TId>
    where TEntity : IEntity<TId>
    where TId : IEquatable<TId>
{
    long Count(CancellationToken cancellationToken = default);

    Task<long> CountAsync(CancellationToken cancellationToken = default);

    Task<long> CountAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);

    TEntity Create(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    void CreateMany(List<TEntity> entities, CancellationToken cancellationToken = default);

    Task CreateManyAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    DeleteResult Delete(TId id, CancellationToken cancellationToken = default);

    Task<DeleteResult> DeleteAsync(TId id, CancellationToken cancellationToken = default);

    Task<DeleteResult> DeleteManyAsync(
        List<TId> ids,
        CancellationToken cancellationToken = default
    );

    Task<DeleteResult> DeleteManyAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    ReplaceOneResult Update(TId id, TEntity entity, CancellationToken cancellationToken = default);

    Task<ReplaceOneResult> UpdateAsync(
        TId id,
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult<TEntity>?> BulkUpsertAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    Task<BulkWriteResult<TEntity>?> BulkUpdateAsync(
        List<TEntity> entities,
        bool isUpsert = false,
        CancellationToken cancellationToken = default
    );

    TEntity? FindOneById(TId id, CancellationToken cancellationToken = default);

    TResultEntity? FindOneById<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FindOneByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<TResultEntity?> FindOneByIdAsync<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    TEntity? FindOneBy(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    );

    TResultEntity? FindOneBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FindOneByAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    );

    Task<TResultEntity?> FindOneByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    List<TEntity> FindBy(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    );

    List<TResultEntity> FindBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    Task<List<TEntity>> FindByAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    );

    Task<List<TResultEntity>> FindByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    );

    Task<List<TEntity>> SkipAndTakeAsync(
        int skip,
        int take,
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    );

    Task<List<TResultEntity>> SkipAndTakeAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );
}