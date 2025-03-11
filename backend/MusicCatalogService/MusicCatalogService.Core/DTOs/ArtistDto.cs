namespace MusicCatalogService.Core.DTOs;

public class ArtistSummaryDto : BaseSpotifyItemDto
{
    // All necessary fields already in base class
}

public class ArtistDetailDto : ArtistSummaryDto
{
    public List<string> Genres { get; set; } = new List<string>();
    public int FollowersCount { get; set; }
    public List<AlbumSummaryDto> TopAlbums { get; set; } = new List<AlbumSummaryDto>();
    public List<TrackSummaryDto> TopTracks { get; set; } = new List<TrackSummaryDto>();
}
