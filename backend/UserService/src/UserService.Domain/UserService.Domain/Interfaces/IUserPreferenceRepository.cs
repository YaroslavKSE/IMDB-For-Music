using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserPreferenceRepository
{
    Task<Dictionary<PreferenceType, List<string>>> GetUserPreferencesAsync(Guid userId);
    Task<bool> UserPreferenceExistsAsync(Guid userId, PreferenceType itemType, string spotifyId);
    Task AddAsync(UserPreference preference);
    Task AddRangeAsync(IEnumerable<UserPreference> preferences);
    Task RemoveAsync(Guid userId, PreferenceType itemType, string spotifyId);
    Task<List<string>> GetUserPreferencesByTypeAsync(Guid userId, PreferenceType type);
    Task<int> GetUserPreferencesCountByTypeAsync(Guid userId, PreferenceType type);
    Task ClearUserPreferencesByTypeAsync(Guid userId, PreferenceType type);
    Task SaveChangesAsync();
}