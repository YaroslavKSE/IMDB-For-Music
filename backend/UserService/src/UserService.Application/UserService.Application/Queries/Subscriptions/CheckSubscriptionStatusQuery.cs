using MediatR;

namespace UserService.Application.Queries.Subscriptions;

public class CheckSubscriptionStatusQuery : IRequest<bool>
{
    public Guid FollowerId { get; }
    public Guid FollowedId { get; }

    public CheckSubscriptionStatusQuery(Guid followerId, Guid followedId)
    {
        FollowerId = followerId;
        FollowedId = followedId;
    }
}