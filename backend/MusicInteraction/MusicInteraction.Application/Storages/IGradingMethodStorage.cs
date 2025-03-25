namespace MusicInteraction.Application.Interfaces;

using MusicInteraction.Domain;

public interface IGradingMethodStorage
{
    Task<bool> IsEmpty();
    Task AddGradingMethodAsync(GradingMethod gradingMethod);
    Task UpdateGradingMethodAsync(GradingMethod gradingMethod);
    Task<List<GradingMethod>> GetPublicGradingMethods();
    Task<List<GradingMethod>> GetUserGradingMethods(string userId);
    Task<GradingMethod> GetGradingMethodById(Guid methodId);
    Task DeleteGradingMethodAsync(Guid Id);
}