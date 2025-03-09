namespace MusicCatalogService.Core.Models;

public class CatalogItem
{
    public Guid Id { get; set; }
    public string SpotifyId { get; set; }
    public string Type { get; set; } // "album", "track", "artist"
    public string Name { get; set; }
    public string? ArtistName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? Popularity { get; set; }
    public DateTime LastAccessed { get; set; }
    public DateTime CacheExpiresAt { get; set; }
}
