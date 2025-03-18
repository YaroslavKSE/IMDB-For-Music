using MediatR;

namespace UserService.Application.Commands;

public class LogoutCommand : IRequest<bool>
{
    public string RefreshToken { get; }
    
    public LogoutCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}