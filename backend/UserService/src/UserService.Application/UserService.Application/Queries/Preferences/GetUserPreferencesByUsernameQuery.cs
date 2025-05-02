using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Preferences;

public class GetUserPreferencesByUsernameQuery : IRequest<UserPreferencesResponse>
{
    public string Username { get; }

    public GetUserPreferencesByUsernameQuery(string username)
    {
        Username = username;
    }
}