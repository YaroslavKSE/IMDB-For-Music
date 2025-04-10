using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class SubscribeToUserCommand : IRequest<SubscriptionResponseDto>
{
    public Guid FollowerId { get; }
    public Guid FollowedId { get; }

    public SubscribeToUserCommand(Guid followerId, Guid followedId)
    {
        FollowerId = followerId;
        FollowedId = followedId;
    }
}