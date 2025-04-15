using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class DeleteUserAvatarCommand : IRequest<UserResponse>
{
    public string Auth0UserId { get; }

    public DeleteUserAvatarCommand(string auth0UserId)
    {
        Auth0UserId = auth0UserId;
    }
}