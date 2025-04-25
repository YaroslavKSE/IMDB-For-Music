namespace MusicCatalogService.Core.Models;

public class Artist : CatalogItemBase
{
    public List<string> Genres { get; set; } = new();
    
    public int? FollowersCount { get; set; }
    
    // External URLs
    public string SpotifyUrl { get; set; }
    
    // Popular track IDs
    public List<string> TopTrackIds { get; set; } = new();
    
    // Related artist IDs
    public List<string> RelatedArtistIds { get; set; } = new();
    
    // Store album IDs for quicker retrieval
    public List<string> AlbumIds { get; set; } = new();
}