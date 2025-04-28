using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly AppDbContext _context;

    public UserPreferenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<PreferenceType, List<string>>> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _context.UserPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var result = new Dictionary<PreferenceType, List<string>>();

        // Initialize dictionary with empty lists for each preference type
        foreach (PreferenceType type in Enum.GetValues(typeof(PreferenceType))) result[type] = new List<string>();

        // Group preferences by type
        foreach (var preference in preferences) result[preference.ItemType].Add(preference.SpotifyId);

        return result;
    }

    public async Task<bool> UserPreferenceExistsAsync(Guid userId, PreferenceType itemType, string spotifyId)
    {
        return await _context.UserPreferences
            .AnyAsync(p => p.UserId == userId && p.ItemType == itemType && p.SpotifyId == spotifyId);
    }

    public async Task AddAsync(UserPreference preference)
    {
        await _context.UserPreferences.AddAsync(preference);
    }

    public async Task AddRangeAsync(IEnumerable<UserPreference> preferences)
    {
        await _context.UserPreferences.AddRangeAsync(preferences);
    }

    public async Task RemoveAsync(Guid userId, PreferenceType itemType, string spotifyId)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ItemType == itemType && p.SpotifyId == spotifyId);

        if (preference != null) _context.UserPreferences.Remove(preference);
    }

    public async Task<List<string>> GetUserPreferencesByTypeAsync(Guid userId, PreferenceType type)
    {
        return await _context.UserPreferences
            .Where(p => p.UserId == userId && p.ItemType == type)
            .Select(p => p.SpotifyId)
            .ToListAsync();
    }

    public async Task<int> GetUserPreferencesCountByTypeAsync(Guid userId, PreferenceType type)
    {
        return await _context.UserPreferences
            .CountAsync(p => p.UserId == userId && p.ItemType == type);
    }

    public async Task ClearUserPreferencesByTypeAsync(Guid userId, PreferenceType type)
    {
        var preferences = await _context.UserPreferences
            .Where(p => p.UserId == userId && p.ItemType == type)
            .ToListAsync();

        _context.UserPreferences.RemoveRange(preferences);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}