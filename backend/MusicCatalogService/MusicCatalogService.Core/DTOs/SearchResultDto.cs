namespace MusicCatalogService.Core.DTOs;

public class SearchResultDto
{
    public string Query { get; set; }
    public string Type { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int TotalResults { get; set; }
    public List<AlbumSummaryDto> Albums { get; set; } = new List<AlbumSummaryDto>();
    public List<TrackSummaryDto> Tracks { get; set; } = new List<TrackSummaryDto>();
    public List<ArtistSummaryDto> Artists { get; set; } = new List<ArtistSummaryDto>();
}