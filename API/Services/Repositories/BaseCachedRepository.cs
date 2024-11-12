using System.Linq.Expressions;
using API.Models.Application;
using API.Models.Application.Interfaces;
using API.Models.Database.Interfaces;
using API.Services.Interfaces;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace API.Services.Repositories;

public abstract class BaseCachedRepository<TDecorated, TEntity, TId>
    : ICachedRepository<TEntity, TId>
    where TDecorated : IRepositoryService<TEntity, TId>
    where TEntity : IEntity<TId>
    where TId : IEquatable<TId>
{
    protected TimeSpan CacheDuration { get; init; } = TimeSpan.FromHours(1);
    protected TimeSpan FilteredDataCacheDuration { get; init; } = TimeSpan.FromMinutes(5);
    protected TDecorated Decorated { get; init; }
    protected IAppCache Cache { get; init; }
    private readonly string _prefixKey;
    public virtual string AllEntitiesKey { get; init; }

    protected BaseCachedRepository(string prefixKey, TDecorated decorated, IAppCache cache)
    {
        Decorated = decorated;
        Cache = cache;
        _prefixKey = prefixKey;
        AllEntitiesKey = $"{_prefixKey}:[all]";
    }

    public virtual string GetKey(TId id)
    {
        return $"{_prefixKey}:[id]={id}";
    }

    public virtual string GetKey(Expression<Func<TEntity, bool>> expression)
    {
        return $"{_prefixKey}:[exp]={ExpressionKeyGenerator.GetKey(expression)}";
    }

    public virtual string GetKey(Expression<Func<TEntity, bool>>? expression, int skip, int take)
    {
        if (expression == null)
        {
            return $"{_prefixKey}:[all]:[skip]={skip}:[take]={take}";
        }

        return $"{_prefixKey}:[exp]={ExpressionKeyGenerator.GetKey(expression)}:[skip]={skip}:[take]={take}";
    }

    public virtual void SetCache(TEntity entity)
    {
        var key = GetKey(entity.Id);
        SetCacheByKey(key, entity);
    }

    public virtual void SetCache(Expression<Func<TEntity, bool>> expression, TEntity entity)
    {
        var key = GetKey(expression);
        SetCacheByKey(key, entity);
    }

    public virtual void SetCacheByKey(string key, TEntity entity)
    {
        Cache.Remove(key);
        Cache.Add(key, entity, CacheDuration);
    }

    public virtual void SetCacheByKey(string key, List<TEntity> entities)
    {
        Cache.Remove(key);
        Cache.Add(key, entities, CacheDuration);
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual long Count(CancellationToken cancellationToken)
    {
        return Decorated.Count(cancellationToken);
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<long> CountAsync(CancellationToken cancellationToken)
    {
        return Decorated.CountAsync(cancellationToken);
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<long> CountAsync(FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return Decorated.CountAsync(filter, cancellationToken);
    }

    public virtual TEntity Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = Decorated.Create(entity, cancellationToken);
        HandleAfterCreate(result);
        return result;
    }

    public virtual async Task<TEntity> CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.CreateAsync(entity, cancellationToken);
        HandleAfterCreate(result);
        return result;
    }

    private void HandleAfterCreate(TEntity result)
    {
        var key = GetKey(result.Id);
        SetCacheByKey(key, result);
        AddToAllEntitiesCache(result);
    }

    private void AddToAllEntitiesCache(TEntity entity)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        list.Add(entity);
        SetCacheByKey(AllEntitiesKey, list);
    }

    private void AddToAllEntitiesCache(List<TEntity> entities)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        list.AddRange(entities);
        SetCacheByKey(AllEntitiesKey, list);
    }

    private void UpdateAllEntitiesCache(TEntity entity)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        var cachedEntityId = list.FindIndex(x => x.Id.Equals(entity.Id));
        if (cachedEntityId == -1)
            return;

        list[cachedEntityId] = entity;
        SetCacheByKey(AllEntitiesKey, list);
    }

    private void UpdateAllEntitiesCache(List<TEntity> entities)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        var entityDict = list.ToDictionary(x => x.Id, x => x);
        bool updated = false;

        foreach (var entity in entities)
        {
            if (entityDict.TryGetValue(entity.Id, out var existingEntity) == false)
                continue;

            entityDict[entity.Id] = entity;
            updated = true;
        }

        if (updated == false)
            return;

        var updatedList = entityDict.Values.ToList();
        SetCacheByKey(AllEntitiesKey, updatedList);
    }

    private void RemoveFromAllEntitiesCache(TId id)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        RemoveFromAllEntitiesAndSetCache(x => x.Id.Equals(id), list);
    }

    private void RemoveFromAllEntitiesCache(List<TId> ids)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        RemoveFromAllEntitiesAndSetCache(x => ids.Contains(x.Id), list);
    }

    private void RemoveFromAllEntitiesCache(List<TEntity> entities)
    {
        if (Cache.TryGetValue(AllEntitiesKey, out List<TEntity>? list) == false || list == null)
            return;

        RemoveFromAllEntitiesAndSetCache(x => entities.Any(e => e.Id.Equals(x.Id)), list);
    }

    private void RemoveFromAllEntitiesAndSetCache(
        Predicate<TEntity> removeMatch,
        List<TEntity> list
    )
    {
        var needUpdate = list.RemoveAll(removeMatch) > 0;
        if (needUpdate == false)
            return;

        SetCacheByKey(AllEntitiesKey, list);
    }

    public virtual void CreateMany(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        Decorated.CreateMany(entities, cancellationToken);
        AddToAllEntitiesCache(entities);
    }

    public virtual async Task CreateManyAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        await Decorated.CreateManyAsync(entities, cancellationToken);
        AddToAllEntitiesCache(entities);
    }

    private void HandleCacheDelete(TId id)
    {
        var key = GetKey(id);
        Cache.Remove(key);
        RemoveFromAllEntitiesCache(id);
    }

    public virtual DeleteResult Delete(TId id, CancellationToken cancellationToken = default)
    {
        var result = Decorated.Delete(id, cancellationToken);
        HandleCacheDelete(id);
        return result;
    }

    public virtual async Task<DeleteResult> DeleteAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.DeleteAsync(id, cancellationToken);
        HandleCacheDelete(id);
        return result;
    }

    public virtual async Task<DeleteResult> DeleteManyAsync(
        List<TId> ids,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.DeleteManyAsync(ids, cancellationToken);
        RemoveFromAllEntitiesCache(ids);

        foreach (var id in ids)
        {
            var key = GetKey(id);
            Cache.Remove(key);
        }

        return result;
    }

    public virtual async Task<DeleteResult> DeleteManyAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.DeleteManyAsync(entities, cancellationToken);
        RemoveFromAllEntitiesCache(entities);

        foreach (var entity in entities)
        {
            var key = GetKey(entity.Id);
            Cache.Remove(key);
        }

        return result;
    }

    /// <param name="expression">Expression to filter data.</param>
    /// <returns>Filtered data or all data if <paramref name="expression"/> is null.</returns>
    public virtual List<TEntity> FindBy(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        var key = expression is null ? AllEntitiesKey : GetKey(expression);
        return Cache.GetOrAdd(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(
                    expression is null ? CacheDuration : FilteredDataCacheDuration
                );
                return Decorated.FindBy(expression);
            }
        )!;
    }

    /// <param name="expression">Expression to filter data.</param>
    /// <returns>Filtered data or all data if <paramref name="expression"/> is null.</returns>
    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual List<TResultEntity> FindBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindBy(aggregateOperator, cancellationToken);
    }

    /// <param name="expression">Expression to filter data.</param>
    /// <returns>Filtered data or all data if <paramref name="expression"/> is null.</returns>
    public virtual Task<List<TEntity>> FindByAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        var key = expression is null ? AllEntitiesKey : GetKey(expression);
        return Cache.GetOrAddAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(
                    expression is null ? CacheDuration : FilteredDataCacheDuration
                );
                return Decorated.FindByAsync(expression, cancellationToken);
            }
        )!;
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<List<TResultEntity>> FindByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindByAsync(aggregateOperator, cancellationToken);
    }

    public virtual TEntity? FindOneBy(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(expression);
        return Cache.GetOrAdd(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(FilteredDataCacheDuration);
                return Decorated.FindOneBy(expression);
            }
        );
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual TResultEntity? FindOneBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindOneBy(aggregateOperator, cancellationToken);
    }

    public virtual Task<TEntity?> FindOneByAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(expression);
        return Cache.GetOrAddAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(FilteredDataCacheDuration);
                return Decorated.FindOneByAsync(expression, cancellationToken);
            }
        );
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<TResultEntity?> FindOneByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindOneByAsync(aggregateOperator, cancellationToken);
    }

    public virtual TEntity? FindOneById(TId id, CancellationToken cancellationToken = default)
    {
        var key = GetKey(id);
        return Cache.GetOrAdd(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(CacheDuration);
                return Decorated.FindOneById(id);
            }
        );
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual TResultEntity? FindOneById<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindOneById(id, aggregateOperator, cancellationToken);
    }

    public virtual Task<TEntity?> FindOneByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(id);
        return Cache.GetOrAddAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(CacheDuration);
                return Decorated.FindOneByIdAsync(id, cancellationToken);
            }
        );
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<TResultEntity?> FindOneByIdAsync<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken
    )
    {
        return Decorated.FindOneByIdAsync(id, aggregateOperator, cancellationToken);
    }

    public virtual ReplaceOneResult Update(
        TId id,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var result = Decorated.Update(id, entity, cancellationToken);
        var key = GetKey(id);
        SetCacheByKey(key, entity);
        UpdateAllEntitiesCache(entity);
        return result;
    }

    public virtual async Task<ReplaceOneResult> UpdateAsync(
        TId id,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.UpdateAsync(id, entity, cancellationToken);
        var key = GetKey(id);
        SetCacheByKey(key, entity);
        UpdateAllEntitiesCache(entity);
        return result;
    }

    /// <summary>
    /// Updates the list of entities and inserts entities that don't exist by sending multiple replace operations in a single request,
    /// reducing the number of round trips to the database and improving performance.
    /// </summary>
    /// <param name="entities">Entities to upsert.</param>
    /// <returns>The result of bulk write operation or null if no upsert has occurred.</returns>
    public virtual async Task<BulkWriteResult<TEntity>?> BulkUpsertAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        var result = await BulkUpdateAsync(entities, true, cancellationToken);
        if (result is null)
            return null;

        foreach (var entity in entities)
        {
            SetCache(entity);
        }

        UpdateAllEntitiesCache(entities);
        return result;
    }

    /// <summary>
    /// Updates a list of entities by sending multiple replace operations in a single request,
    /// reducing the number of round trips to the database and improving performance.
    /// </summary>
    /// <param name="entities">Entities to update.</param>
    /// <param name="isUpsert">Gets or sets a value indicating whether to insert an entity if it does not already exist.</param>
    /// <returns>The result of bulk write operation or null if no update has occurred.</returns>
    public virtual async Task<BulkWriteResult<TEntity>?> BulkUpdateAsync(
        List<TEntity> entities,
        bool isUpsert = false,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Decorated.BulkUpdateAsync(entities, isUpsert, cancellationToken);
        if (result is null)
            return null;

        foreach (var entity in entities)
        {
            SetCache(entity);
        }

        UpdateAllEntitiesCache(entities);
        return result;
    }

    public virtual Task<List<TEntity>> SkipAndTakeAsync(
        int skip,
        int take,
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetKey(expression, skip, take);
        return Cache.GetOrAddAsync(
            key,
            entity =>
            {
                entity.SetAbsoluteExpiration(FilteredDataCacheDuration);
                return Decorated.SkipAndTakeAsync(skip, take, expression, cancellationToken);
            }
        );
    }

    /// <remarks>
    /// This method does not support caching.
    /// </remarks>
    public virtual Task<List<TResultEntity>> SkipAndTakeAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        int skip,
        int take,
        CancellationToken cancellationToken
    )
    {
        return Decorated.SkipAndTakeAsync(aggregateOperator, skip, take, cancellationToken);
    }
}