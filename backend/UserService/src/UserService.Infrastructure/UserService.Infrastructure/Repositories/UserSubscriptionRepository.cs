using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly AppDbContext _context;

    public UserSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription> GetSubscriptionAsync(Guid followerId, Guid followedId)
    {
        return await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.FollowerId == followerId && s.FollowedId == followedId);
    }

    public async Task<List<UserSubscription>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        return await _context.UserSubscriptions
            .Where(s => s.FollowedId == userId)
            .Include(s => s.Follower)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<UserSubscription>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        return await _context.UserSubscriptions
            .Where(s => s.FollowerId == userId)
            .Include(s => s.Followed)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCountAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .CountAsync(s => s.FollowedId == userId);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .CountAsync(s => s.FollowerId == userId);
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
    {
        return await _context.UserSubscriptions
            .AnyAsync(s => s.FollowerId == followerId && s.FollowedId == followedId);
    }

    public async Task<Dictionary<Guid, bool>> AreBatchFollowingAsync(Guid followerId, List<Guid> followedIds)
    {
        // Get all subscriptions that exist between this follower and any of the followed IDs
        var existingSubscriptions = await _context.UserSubscriptions
            .Where(s => s.FollowerId == followerId && followedIds.Contains(s.FollowedId))
            .Select(s => s.FollowedId)
            .ToListAsync();

        // Build result dictionary
        var result = new Dictionary<Guid, bool>();
        foreach (var followedId in followedIds) result[followedId] = existingSubscriptions.Contains(followedId);

        return result;
    }

    public async Task<List<Guid>> GetFollowerIdsAsync(Guid userId, int page, int pageSize)
    {
        return await _context.UserSubscriptions
            .Where(s => s.FollowedId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.FollowerId)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetFollowingIdsAsync(Guid userId, int page, int pageSize)
    {
        return await _context.UserSubscriptions
            .Where(s => s.FollowerId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.FollowedId)
            .ToListAsync();
    }


    public async Task AddAsync(UserSubscription subscription)
    {
        await _context.UserSubscriptions.AddAsync(subscription);
    }

    public async Task RemoveAsync(UserSubscription subscription)
    {
        _context.UserSubscriptions.Remove(subscription);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}