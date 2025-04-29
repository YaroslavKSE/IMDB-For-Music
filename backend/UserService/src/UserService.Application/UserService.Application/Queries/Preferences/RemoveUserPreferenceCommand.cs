using MediatR;

namespace UserService.Application.Queries.Preferences;

public class RemoveUserPreferenceCommand : IRequest<bool>
{
    public string Auth0UserId { get; }
    public string ItemType { get; }
    public string SpotifyId { get; }

    public RemoveUserPreferenceCommand(string auth0UserId, string itemType, string spotifyId)
    {
        Auth0UserId = auth0UserId;
        ItemType = itemType;
        SpotifyId = spotifyId;
    }
}