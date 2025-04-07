namespace MusicCatalogService.Core.Models;

public class Artist : CatalogItemBase
{
    public List<string> Genres { get; set; } = new();
    public int? FollowersCount { get; set; }
    // External URLs
    public string SpotifyUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}