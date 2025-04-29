using MediatR;

namespace UserService.Application.Queries.Preferences;

public class BulkAddUserPreferencesCommand : IRequest<bool>
{
    public string Auth0UserId { get; }
    public List<string> Artists { get; }
    public List<string> Albums { get; }
    public List<string> Tracks { get; }

    public BulkAddUserPreferencesCommand(
        string auth0UserId,
        List<string> artists,
        List<string> albums,
        List<string> tracks)
    {
        Auth0UserId = auth0UserId;
        Artists = artists ?? new List<string>();
        Albums = albums ?? new List<string>();
        Tracks = tracks ?? new List<string>();
    }
}