using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class UpdateUserProfileCommand : IRequest<UserResponse>
{
    public string Auth0UserId { get; }
    public string? Username { get; }
    public string? Name { get; }
    public string? Surname { get; }
    public string? Bio { get; }

    public UpdateUserProfileCommand(string auth0UserId, string? username, string? name, string? surname, string? bio)
    {
        Auth0UserId = auth0UserId;
        Username = username;
        Name = name;
        Surname = surname;
        Bio = bio;
    }
}