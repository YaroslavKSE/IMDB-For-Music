using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Subscriptions;

public class GetUserFollowersQuery : IRequest<PaginatedSubscriptionsResponseDto>
{
    public Guid UserId { get; }
    public int Page { get; }
    public int PageSize { get; }

    public GetUserFollowersQuery(Guid userId, int page, int pageSize)
    {
        UserId = userId;
        Page = page;
        PageSize = pageSize;
    }
}