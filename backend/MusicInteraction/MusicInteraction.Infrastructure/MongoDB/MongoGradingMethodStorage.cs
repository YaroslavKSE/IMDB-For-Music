using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.MongoDB;
using MusicInteraction.Infrastructure.MongoDB.Entities;
using MusicInteraction.Infrastructure.MongoDB.Mapping;

namespace MusicInteraction.Infrastructure.MongoDB;

public class MongoGradingMethodStorage : IGradingMethodStorage
{
    private readonly IGradingMethodRepository _repository;

    public MongoGradingMethodStorage(IGradingMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task AddGradingMethodAsync(GradingMethod gradingMethod)
    {
        var entity = gradingMethod.ToEntity();
        await _repository.CreateAsync(entity);
    }

    public async Task<GradingMethod> GetGradingMethodById(Guid methodId)
    {
        var entity = await _repository.GetByIdAsync(methodId);
        return entity.ToDomain();
    }

    public async Task<List<GradingMethod>> GetPublicGradingMethods()
    {
        var entities = await _repository.GetPublicGradingMethodsAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<GradingMethod>> GetUserGradingMethods(string userId)
    {
        var entities = await _repository.GetUserGradingMethodsAsync(userId);
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<bool> IsEmpty()
    {
        return await _repository.CollectionIsEmpty();
    }
}