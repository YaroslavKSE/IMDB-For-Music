using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Users;

public class GetUserProfileQuery : IRequest<UserResponse>
{
    public string Auth0UserId { get; }

    public GetUserProfileQuery(string auth0UserId)
    {
        Auth0UserId = auth0UserId;
    }
}