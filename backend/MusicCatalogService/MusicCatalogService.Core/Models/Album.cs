namespace MusicCatalogService.Core.Models;

public class Album : CatalogItem
{
    public string ArtistName { get; set; }
    public string ReleaseDate { get; set; }
    public string AlbumType { get; set; } // e.g., "album", "single", "compilation"
    
    // Navigation properties
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
