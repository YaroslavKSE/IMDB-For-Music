namespace MusicCatalogService.Core.DTOs;

public class NewReleasesDto
{
    public List<AlbumDto> Albums { get; set; } = new List<AlbumDto>();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}