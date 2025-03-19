using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class SocialLoginCommand : IRequest<LoginResponseDto>
{
    public string AccessToken { get; }
    public string Provider { get; }

    public SocialLoginCommand(string accessToken, string provider)
    {
        AccessToken = accessToken;
        Provider = provider;
    }
}