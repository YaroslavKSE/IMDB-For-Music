namespace MusicCatalogService.Core.DTOs;

public class AlbumTracksResultDto
{
    public string AlbumId { get; set; }
    public string AlbumName { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int TotalResults { get; set; }
    public string Next { get; set; }
    public string Previous { get; set; }
    public List<TrackSummaryDto> Tracks { get; set; } = new();
}