namespace MusicCatalogService.Core.DTOs;

public class TrackDto
{
    public string SpotifyId { get; set; }
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public int DurationMs { get; set; }
}
