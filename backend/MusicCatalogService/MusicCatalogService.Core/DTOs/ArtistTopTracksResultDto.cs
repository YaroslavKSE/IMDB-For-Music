namespace MusicCatalogService.Core.DTOs;

public class ArtistTopTracksResultDto
{
    public string ArtistId { get; set; }
    public string ArtistName { get; set; }
    public string Market { get; set; }
    public List<TrackSummaryDto> Tracks { get; set; } = new();
}