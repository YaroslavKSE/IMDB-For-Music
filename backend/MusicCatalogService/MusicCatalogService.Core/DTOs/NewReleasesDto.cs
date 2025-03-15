namespace MusicCatalogService.Core.DTOs;

public class NewReleasesDto
{
    public List<AlbumSummaryDto> Albums { get; set; } = new List<AlbumSummaryDto>();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}