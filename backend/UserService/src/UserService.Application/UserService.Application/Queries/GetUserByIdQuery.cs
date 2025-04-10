using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public class GetUserByIdQuery : IRequest<UserResponse>
{
    public Guid UserId { get; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}