namespace MusicCatalogService.Core.DTOs;

public class ArtistSummaryDto : BaseSpotifyItemDto
{
    // All necessary fields already in base class
}

public class ArtistDetailDto : ArtistSummaryDto
{
    public List<string> Genres { get; set; } = new();
    public int FollowersCount { get; set; }
    
    // Optional lists of related content
    // These would typically be populated by additional API calls or relationship lookups
    public List<AlbumSummaryDto> TopAlbums { get; set; } = new();
    public List<TrackSummaryDto> TopTracks { get; set; } = new();
}