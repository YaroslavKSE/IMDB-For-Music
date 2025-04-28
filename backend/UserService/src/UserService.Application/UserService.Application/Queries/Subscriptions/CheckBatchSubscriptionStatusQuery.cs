using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Subscriptions;

public class CheckBatchSubscriptionStatusQuery : IRequest<BatchSubscriptionResponseDto>
{
    public Guid FollowerId { get; }
    public List<Guid> TargetUserIds { get; }

    public CheckBatchSubscriptionStatusQuery(Guid followerId, List<Guid> targetUserIds)
    {
        FollowerId = followerId;
        TargetUserIds = targetUserIds;
    }
}