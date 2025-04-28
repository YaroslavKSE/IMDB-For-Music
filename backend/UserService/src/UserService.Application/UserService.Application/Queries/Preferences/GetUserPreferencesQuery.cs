using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Preferences;

public class GetUserPreferencesQuery : IRequest<UserPreferencesResponse>
{
    public string Auth0UserId { get; }

    public GetUserPreferencesQuery(string auth0UserId)
    {
        Auth0UserId = auth0UserId;
    }
}