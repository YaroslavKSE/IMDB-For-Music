using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Subscriptions;

public class GetUserFollowerIdsQuery : IRequest<FollowerIdsResponseDto>
{
    public Guid UserId { get; }
    public int Page { get; }
    public int PageSize { get; }

    public GetUserFollowerIdsQuery(Guid userId, int page, int pageSize)
    {
        UserId = userId;
        Page = page;
        PageSize = pageSize;
    }
}