using MediatR;
using Microsoft.AspNetCore.Http;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class UpdateUserAvatarCommand : IRequest<UserResponse>
{
    public string Auth0UserId { get; }
    public IFormFile File { get; }

    public UpdateUserAvatarCommand(string auth0UserId, IFormFile file)
    {
        Auth0UserId = auth0UserId;
        File = file;
    }
}