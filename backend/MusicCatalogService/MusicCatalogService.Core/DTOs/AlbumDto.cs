using System.Text.Json.Serialization;

namespace MusicCatalogService.Core.DTOs;

public class AlbumSummaryDto : BaseSpotifyItemDto
{
    public string ArtistName { get; set; }
    public string ReleaseDate { get; set; }
    public string AlbumType { get; set; }
    public int? TotalTracks { get; set; }
}

public class AlbumDetailDto : AlbumSummaryDto
{
    public List<ArtistSummaryDto> Artists { get; set; } = new();
    public List<TrackSummaryDto> Tracks { get; set; } = new();
    public string ReleaseDatePrecision { get; set; }
    public string Label { get; set; }
    public string Copyright { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Genres { get; set; }
}