using MediatR;

namespace UserService.Application.Queries.Subscriptions;

public class GetUserFollowersCountQuery : IRequest<int>
{
    public Guid UserId { get; }

    public GetUserFollowersCountQuery(Guid userId)
    {
        UserId = userId;
    }
}