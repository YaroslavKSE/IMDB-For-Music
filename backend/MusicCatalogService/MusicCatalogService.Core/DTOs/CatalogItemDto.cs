namespace MusicCatalogService.Core.DTOs;

public class CatalogItemDto
{
    public Guid CatalogItemId { get; set; }
    public string SpotifyId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public List<ArtistDto> Artists { get; set; }
    public string ImageUrl { get; set; }
    public int? Popularity { get; set; }
    
    // Album-specific properties
    public string ReleaseDate { get; set; }
    public string AlbumType { get; set; }
    public List<TrackDto> Tracks { get; set; }
    
    // Track-specific properties
    public int? DurationMs { get; set; }
    public string AlbumName { get; set; }
    public string AlbumId { get; set; }
    public bool IsExplicit { get; set; }
    
    // Artist-specific properties
    public List<string> Genres { get; set; }
    public int? FollowersCount { get; set; }
}