namespace MusicCatalogService.Core.Models;

public class Track : CatalogItem
{
    public string ArtistName { get; set; }
    public int DurationMs { get; set; }
    public bool IsExplicit { get; set; }
    
    // Foreign key for Album
    public Guid? AlbumId { get; set; }
    
    // Navigation property
    public virtual Album Album { get; set; }
}