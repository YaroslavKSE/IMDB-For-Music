using MediatR;

namespace UserService.Application.Queries.Preferences;

public class ClearUserPreferencesCommand : IRequest<bool>
{
    public string Auth0UserId { get; }
    public string Type { get; }

    public ClearUserPreferencesCommand(string auth0UserId, string type)
    {
        Auth0UserId = auth0UserId;
        Type = type;
    }
}