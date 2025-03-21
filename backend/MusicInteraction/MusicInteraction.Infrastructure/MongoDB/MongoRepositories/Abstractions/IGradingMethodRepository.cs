using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB;

public interface IGradingMethodRepository : IMongoRepository<GradingMethodEntity>
{
    Task<List<GradingMethodEntity>> GetPublicGradingMethodsAsync();
    Task<List<GradingMethodEntity>> GetUserGradingMethodsAsync(string userId);
}