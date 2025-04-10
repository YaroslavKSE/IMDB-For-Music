using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class RegisterUserCommand : IRequest<RegisterUserResponse>
{
    public string Email { get; }
    public string Password { get; }
    public string Username { get; }
    public string Name { get; }
    public string Surname { get; }

    public RegisterUserCommand(string email, string password, string username, string name, string surname)
    {
        Email = email;
        Password = password;
        Username = username;
        Name = name;
        Surname = surname;
    }
}