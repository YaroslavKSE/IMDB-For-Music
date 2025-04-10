namespace UserService.Domain.Entities;

public class UserSubscription
{
    public Guid Id { get; private set; }
    public Guid FollowerId { get; private set; }
    public Guid FollowedId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties for EF Core
    public virtual User Follower { get; private set; }
    public virtual User Followed { get; private set; }

    private UserSubscription()
    {
    } // For EF Core

    public static UserSubscription Create(Guid followerId, Guid followedId)
    {
        return new UserSubscription
        {
            Id = Guid.NewGuid(),
            FollowerId = followerId,
            FollowedId = followedId,
            CreatedAt = DateTime.UtcNow
        };
    }
}