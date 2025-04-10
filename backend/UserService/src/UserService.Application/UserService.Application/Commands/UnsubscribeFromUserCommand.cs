using MediatR;

namespace UserService.Application.Commands;

public class UnsubscribeFromUserCommand : IRequest<bool>
{
    public Guid FollowerId { get; }
    public Guid FollowedId { get; }

    public UnsubscribeFromUserCommand(Guid followerId, Guid followedId)
    {
        FollowerId = followerId;
        FollowedId = followedId;
    }
}