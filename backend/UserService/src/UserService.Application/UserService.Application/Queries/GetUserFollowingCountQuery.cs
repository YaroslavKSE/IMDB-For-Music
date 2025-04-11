using MediatR;

namespace UserService.Application.Queries;
public class GetUserFollowingCountQuery : IRequest<int>
{
    public Guid UserId { get; }

    public GetUserFollowingCountQuery(Guid userId)
    {
        UserId = userId;
    }
}