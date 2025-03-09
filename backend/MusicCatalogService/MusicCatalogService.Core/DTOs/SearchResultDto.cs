namespace MusicCatalogService.Core.DTOs;

public class SearchResultDto
{
    public string Query { get; set; }
    public string Type { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int TotalResults { get; set; }
    public List<AlbumDto> Albums { get; set; } = new List<AlbumDto>();
    public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
    public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
}
