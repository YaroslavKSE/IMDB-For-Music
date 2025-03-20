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