using System.Linq.Expressions;
using API.Models.Database.Interfaces;
using MongoDB.Driver;

namespace API.Services.Interfaces;

public interface ICachedRepository<TEntity, TId> : IRepositoryService<TEntity, TId>
    where TEntity : IEntity<TId>
    where TId : IEquatable<TId>
{
    string GetKey(TId id);

    string GetKey(Expression<Func<TEntity, bool>> expression);

    string GetKey(Expression<Func<TEntity, bool>>? expression, int skip, int take);

    /// <summary>
    /// Saves data to the cache by the generated key, overwriting the existing data.
    /// </summary>
    /// <param name="id">Identifier of the entity that will be used to generate the key.</param>
    /// <param name="entity">Data to save</param>
    void SetCache(TEntity entity);

    /// <summary>
    /// Saves data to the cache by the generated key, overwriting the existing data.
    /// </summary>
    /// <param name="expression">Expression that will be used to generate the key.</param>
    /// <param name="entity">Data to save</param>
    void SetCache(Expression<Func<TEntity, bool>> expression, TEntity entity);

    /// <summary>
    /// Saves data to the cache by the key, overwriting the existing data.
    /// </summary>
    /// <param name="key">Key that will be used to access data from the cache.</param>
    /// <param name="entity">Data to save</param>
    void SetCacheByKey(string key, TEntity entity);

    /// <summary>
    /// Saves data to the cache by the key, overwriting the existing data.
    /// </summary>
    /// <param name="key">Key that will be used to access data from the cache.</param>
    /// <param name="entities">Data to save</param>
    void SetCacheByKey(string key, List<TEntity> entities);
}
