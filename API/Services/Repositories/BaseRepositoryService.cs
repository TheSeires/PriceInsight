using System.Linq.Expressions;
using API.Models.Application.Interfaces;
using API.Models.Database.Interfaces;
using API.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace API.Services.Repositories;

public abstract class BaseRepositoryService<TEntity, TId> : IRepositoryService<TEntity, TId>
    where TEntity : IEntity<TId>
    where TId : IEquatable<TId>
{
    protected abstract IMongoCollection<TEntity> Collection { get; init; }
    protected IMongoDatabase MongoDatabase { get; init; }
    public virtual Expression<Func<TEntity, bool>> FindAllExp { get; init; } = x => true;

    public BaseRepositoryService(IMongoDatabase mongoDatabase)
    {
        MongoDatabase = mongoDatabase;
    }

    public virtual long Count(CancellationToken cancellationToken = default)
    {
        return Collection.EstimatedDocumentCount(cancellationToken: cancellationToken);
    }

    public virtual Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return Collection.EstimatedDocumentCountAsync(cancellationToken: cancellationToken);
    }

    public virtual Task<long> CountAsync(FilterDefinition<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        return Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public virtual TEntity Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        Collection.InsertOne(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity> CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual void CreateMany(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        if (entities.Count == 0)
            return;

        Collection.InsertMany(entities, cancellationToken: cancellationToken);
    }

    public virtual Task CreateManyAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        if (entities.Count == 0)
            return Task.CompletedTask;

        return Collection.InsertManyAsync(entities, cancellationToken: cancellationToken);
    }

    public virtual DeleteResult Delete(TId id, CancellationToken cancellationToken = default)
    {
        return Collection.DeleteOne(x => x.Id.Equals(id), cancellationToken);
    }

    public virtual Task<DeleteResult> DeleteAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return Collection.DeleteOneAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public virtual Task<DeleteResult> DeleteManyAsync(
        List<TId> ids,
        CancellationToken cancellationToken = default
    )
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);
        return Collection.DeleteManyAsync(filter, cancellationToken);
    }

    public virtual Task<DeleteResult> DeleteManyAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, entities.Select(e => e.Id));
        return Collection.DeleteManyAsync(filter, cancellationToken);
    }

    public virtual ReplaceOneResult Update(
        TId id,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return Collection.ReplaceOne(
            x => x.Id.Equals(id),
            entity,
            cancellationToken: cancellationToken
        );
    }

    public virtual Task<ReplaceOneResult> UpdateAsync(
        TId id,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return Collection.ReplaceOneAsync(
            x => x.Id.Equals(id),
            entity,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates the list of entities and inserts entities that don't exist by sending multiple replace operations in a single request,
    /// reducing the number of round trips to the database and improving performance.
    /// </summary>
    /// <param name="entities">Entities to upsert.</param>
    /// <returns>The result of bulk write operation or null if no upsert has occurred.</returns>
    public virtual Task<BulkWriteResult<TEntity>?> BulkUpsertAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        return BulkUpdateAsync(entities, true, cancellationToken);
    }

    /// <summary>
    /// Updates a list of entities by sending multiple replace operations in a single request,
    /// reducing the number of round trips to the database and improving performance.
    /// </summary>
    /// <param name="entities">Entities to update.</param>
    /// <param name="isUpsert">Gets or sets a value indicating whether to insert an entity if it does not already exist.</param>
    /// <returns>The result of bulk write operation or null if no update has occurred.</returns>
    public virtual Task<BulkWriteResult<TEntity>?> BulkUpdateAsync(
        List<TEntity> entities,
        bool isUpsert = false,
        CancellationToken cancellationToken = default
    )
    {
        var bulkOps = new List<WriteModel<TEntity>>();

        foreach (var entity in entities)
        {
            var filter = Builders<TEntity>.Filter.Eq(p => p.Id, entity.Id);
            var replaceOne = new ReplaceOneModel<TEntity>(filter, entity) { IsUpsert = isUpsert };

            bulkOps.Add(replaceOne);
        }

        if (bulkOps.Count != 0)
        {
            return Collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
        }

        return Task.FromResult<BulkWriteResult<TEntity>?>(null);
    }

    public virtual TEntity? FindOneById(TId id, CancellationToken cancellationToken = default)
    {
        var query = Collection.Find(x => x.Id.Equals(id));
        return query.FirstOrDefault(cancellationToken);
    }

    public virtual TResultEntity? FindOneById<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(id, aggregateOperator, cancellationToken);
        return aggregate.FirstOrDefault(cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        var query = await Collection.FindAsync(
            x => x.Id.Equals(id),
            cancellationToken: cancellationToken
        );
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TResultEntity?> FindOneByIdAsync<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(id, aggregateOperator, cancellationToken);
        return await aggregate.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual TEntity? FindOneBy(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    )
    {
        var query = Collection.Find(expression);
        return query.FirstOrDefault(cancellationToken);
    }

    public virtual TResultEntity? FindOneBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(aggregateOperator, cancellationToken);
        return aggregate.FirstOrDefault(cancellationToken);
    }

    public virtual async Task<TEntity?> FindOneByAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default
    )
    {
        var query = await Collection.FindAsync(expression, cancellationToken: cancellationToken);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TResultEntity?> FindOneByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(aggregateOperator, cancellationToken);
        return await aggregate.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual List<TEntity> FindBy(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Collection.Find(expression ?? FindAllExp);
        return query.ToList(cancellationToken);
    }

    public virtual List<TResultEntity> FindBy<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(aggregateOperator, cancellationToken);
        return aggregate.ToList(cancellationToken);
    }

    /// <param name="expression">Expression to filter data.</param>
    /// <returns>Filtered data or all data if <paramref name="expression"/> is null.</returns>
    public virtual async Task<List<TEntity>> FindByAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = await Collection.FindAsync(
            expression ?? FindAllExp,
            cancellationToken: cancellationToken
        );
        return await query.ToListAsync(cancellationToken);
    }

    /// <param name="expression">Expression to filter data.</param>
    /// <returns>Filtered data or all data if <paramref name="expression"/> is null.</returns>
    public virtual Task<List<TResultEntity>> FindByAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var aggregate = Aggregate(aggregateOperator, cancellationToken);
        return aggregate.ToListAsync(cancellationToken);
    }

    public virtual Task<List<TEntity>> SkipAndTakeAsync(
        int skip,
        int take,
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default
    )
    {
        if ((skip == 0 && take == 0) || take == 0)
            return Task.FromResult<List<TEntity>>([]);

        var fluentFind = Collection.Find(expression ?? FindAllExp);

        if (skip > 0)
        {
            fluentFind = fluentFind.Skip(skip);
        }

        if (take > 0)
        {
            fluentFind = fluentFind.Limit(take);
        }

        return fluentFind.ToListAsync(cancellationToken);
    }

    public virtual Task<List<TResultEntity>> SkipAndTakeAsync<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        if ((skip == 0 && take == 0) || take == 0)
            return Task.FromResult<List<TResultEntity>>([]);

        var pipeline = aggregateOperator.Skip(skip).Take(take).GetPipeline();

        return Collection
            .Aggregate<TResultEntity>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
    }

    protected virtual IAsyncCursor<TResultEntity> Aggregate<TResultEntity>(
        TEntity localEntity,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        return Aggregate(localEntity.Id, aggregateOperator, cancellationToken);
    }

    protected virtual IAsyncCursor<TResultEntity> Aggregate<TResultEntity>(
        TId id,
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        var pipeline = new List<BsonDocument>
        {
            new("$match", new BsonDocument("_id", new BsonObjectId(new ObjectId(id.ToString())))),
        };
        pipeline.AddRange(aggregateOperator.GetPipeline());

        return Collection.Aggregate<TResultEntity>(pipeline, cancellationToken: cancellationToken);
    }

    protected virtual IAsyncCursor<TResultEntity> Aggregate<TResultEntity>(
        IAggregateOperator<TResultEntity> aggregateOperator,
        CancellationToken cancellationToken = default
    )
    {
        return Collection.Aggregate<TResultEntity>(
            aggregateOperator.GetPipeline(),
            cancellationToken: cancellationToken
        );
    }
}