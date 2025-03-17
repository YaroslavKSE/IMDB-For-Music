using MongoDB.Driver;
using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB;

public interface IGradingMethodRepository : IMongoRepository<GradingMethodEntity>
{
    Task<List<GradingMethodEntity>> GetPublicGradingMethodsAsync();
    Task<List<GradingMethodEntity>> GetUserGradingMethodsAsync(string userId);
}

public class GradingMethodRepository : MongoRepository<GradingMethodEntity>, IGradingMethodRepository
{
    private readonly IMongoCollection<GradingMethodEntity> _collection;

    public GradingMethodRepository(MongoDbContext context)
        : base(context, ctx => ctx.GradingMethods)
    {
        _collection = context.GradingMethods;
    }

    public async Task<List<GradingMethodEntity>> GetPublicGradingMethodsAsync()
    {
        var filter = Builders<GradingMethodEntity>.Filter.Eq(gm => gm.IsPublic, true);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<GradingMethodEntity>> GetUserGradingMethodsAsync(string userId)
    {
        var filter = Builders<GradingMethodEntity>.Filter.Eq(gm => gm.CreatorId, userId);
        return await _collection.Find(filter).ToListAsync();
    }
}