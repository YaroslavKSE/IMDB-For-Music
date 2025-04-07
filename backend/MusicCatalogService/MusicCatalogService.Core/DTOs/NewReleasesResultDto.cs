namespace MusicCatalogService.Core.DTOs;

public class NewReleasesResultDto
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int TotalResults { get; set; }
    public string Next { get; set; }
    public string Previous { get; set; }
    public List<AlbumSummaryDto> Albums { get; set; } = new();
}