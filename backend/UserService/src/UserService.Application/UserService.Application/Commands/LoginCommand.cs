using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; }
    public string Password { get; }

    public LoginCommand(string email, string password)
    {
        Email = email;
        Password = password;
    }
}