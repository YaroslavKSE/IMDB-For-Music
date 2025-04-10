using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public class GetUserFollowingQuery : IRequest<PaginatedSubscriptionsResponseDto>
{
    public Guid UserId { get; }
    public int Page { get; }
    public int PageSize { get; }

    public GetUserFollowingQuery(Guid userId, int page, int pageSize)
    {
        UserId = userId;
        Page = page;
        PageSize = pageSize;
    }
}