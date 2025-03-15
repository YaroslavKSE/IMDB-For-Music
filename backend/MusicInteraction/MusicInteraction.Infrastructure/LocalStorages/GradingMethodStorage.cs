using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure.LocalStorages;

public class GradingMethodStorage : IGradingMethodStorage
{
    private readonly LocalDBTemplate Database;

    public GradingMethodStorage(LocalDBTemplate database)
    {
        Database = database;
    }

    public async Task AddGradingMethodAsync(GradingMethod gradingMethod)
    {
        await Database.AddGradingMethod(gradingMethod);
    }

    public async Task<GradingMethod> GetGradingMethodById(Guid methodId)
    {
        return await Database.GetGradingMethodById(methodId);
    }

    public async Task<List<GradingMethod>> GetPublicGradingMethods()
    {
        return await Database.GetPublicGradingMethods();
    }

    public async Task<List<GradingMethod>> GetUserGradingMethods(string userId)
    {
        return await Database.GetUserGradingMethods(userId);
    }

    public async Task<bool> IsEmpty()
    {
        return Database.IsGradingMethodsEmpty();
    }
}