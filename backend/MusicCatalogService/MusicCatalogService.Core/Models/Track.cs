namespace MusicCatalogService.Core.Models;

public class Track : CatalogItemBase
{
    // Track-specific fields
    public int DurationMs { get; set; }

    public bool IsExplicit { get; set; }

    public string Isrc { get; set; }
    
    // Add preview URL if needed
    public string PreviewUrl { get; set; }
    
    // Add track number
    public int TrackNumber { get; set; }
    
    // Add disc number
    public int DiscNumber { get; set; }

    // Album information
    public string AlbumId { get; set; }

    public string AlbumName { get; set; }

    public string AlbumType { get; set; }

    public string ReleaseDate { get; set; }

    // External URLs
    public string SpotifyUrl { get; set; }

    // List of simplified artists
    public List<SimplifiedArtist> Artists { get; set; } = new();
}