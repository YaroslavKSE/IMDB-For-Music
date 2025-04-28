using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Users;

public class GetBatchUsersQuery : IRequest<BatchUserResponseDto>
{
    public List<Guid> UserIds { get; }

    public GetBatchUsersQuery(List<Guid> userIds)
    {
        UserIds = userIds;
    }
}