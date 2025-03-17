using MongoDB.Driver;
using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB;

public interface IMongoRepository<TEntity> where TEntity : MongoEntity
{
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(Guid id);
    Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filter);
    Task CreateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
    Task<bool> CollectionIsEmpty();
}

public class MongoRepository<TEntity> : IMongoRepository<TEntity> where TEntity : MongoEntity
{
    private readonly IMongoCollection<TEntity> _collection;

    public MongoRepository(MongoDbContext context, Func<MongoDbContext, IMongoCollection<TEntity>> collectionSelector)
    {
        _collection = collectionSelector(context);
    }

    public async Task CreateAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _collection.DeleteOneAsync(e => e.Id == id);
    }

    public async Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<TEntity> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _collection.Find(e => e.Id == id).FirstOrDefaultAsync();

            if (result == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found");
            }

            return result;
        }
        catch (Exception ex)
        {
            // Add more diagnostic information
            throw new Exception($"Error retrieving entity with ID {id}: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(TEntity entity)
    {
        await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
    }

    public async Task<bool> CollectionIsEmpty()
    {
        return await _collection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty) == 0;
    }
}