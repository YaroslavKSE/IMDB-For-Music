using MediatR;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands;

public class GetAvatarUploadUrlCommand : IRequest<PresignedUploadResponse>
{
    public string Auth0UserId { get; }
    public string ContentType { get; }

    public GetAvatarUploadUrlCommand(string auth0UserId, string contentType)
    {
        Auth0UserId = auth0UserId;
        ContentType = contentType;
    }
}