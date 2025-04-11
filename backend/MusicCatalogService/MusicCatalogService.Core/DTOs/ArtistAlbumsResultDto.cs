namespace MusicCatalogService.Core.DTOs;

public class ArtistAlbumsResultDto
{
    public string ArtistId { get; set; }
    public string ArtistName { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int TotalResults { get; set; }
    public string Next { get; set; }
    public string Previous { get; set; }
    public List<AlbumSummaryDto> Albums { get; set; } = new();
}