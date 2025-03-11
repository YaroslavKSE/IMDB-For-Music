namespace MusicCatalogService.Core.Models;

public class Artist : CatalogItem
{
    public List<string> Genres { get; set; } = new List<string>();
    public int? FollowersCount { get; set; }
    
    // Navigation properties
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
