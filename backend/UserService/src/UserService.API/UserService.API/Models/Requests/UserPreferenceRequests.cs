namespace UserService.API.Models.Requests;

public class AddPreferenceRequest
{
    public string ItemType { get; set; } // "artist", "album", "track"
    public string SpotifyId { get; set; }
}

public class BulkAddPreferencesRequest
{
    public List<string> Artists { get; set; } = new();
    public List<string> Albums { get; set; } = new();
    public List<string> Tracks { get; set; } = new();
}