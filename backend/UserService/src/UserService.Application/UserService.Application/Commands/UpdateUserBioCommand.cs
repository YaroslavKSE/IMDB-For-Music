using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class UpdateUserBioCommand : IRequest<UserResponse>
{
    public string Auth0UserId { get; }
    public string Bio { get; }

    public UpdateUserBioCommand(string auth0UserId, string bio)
    {
        Auth0UserId = auth0UserId;
        Bio = bio;
    }
}