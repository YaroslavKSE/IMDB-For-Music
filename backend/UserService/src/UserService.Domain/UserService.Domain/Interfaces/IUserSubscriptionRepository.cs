using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription> GetSubscriptionAsync(Guid followerId, Guid followedId);
    Task<List<UserSubscription>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<List<UserSubscription>> GetFollowingAsync(Guid userId, int page, int pageSize);
    Task<int> GetFollowersCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followedId);
    Task<Dictionary<Guid, bool>> AreBatchFollowingAsync(Guid followerId, List<Guid> followedIds);
    Task<List<Guid>> GetFollowerIdsAsync(Guid userId, int page, int pageSize);


    Task AddAsync(UserSubscription subscription);
    Task RemoveAsync(UserSubscription subscription);
    Task SaveChangesAsync();
}