using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public class CompleteAvatarUploadCommand : IRequest<UserResponse>
{
    public string Auth0UserId { get; }
    public string ObjectKey { get; }
    public string AvatarUrl { get; }

    public CompleteAvatarUploadCommand(string auth0UserId, string objectKey, string avatarUrl)
    {
        Auth0UserId = auth0UserId;
        ObjectKey = objectKey;
        AvatarUrl = avatarUrl;
    }
}