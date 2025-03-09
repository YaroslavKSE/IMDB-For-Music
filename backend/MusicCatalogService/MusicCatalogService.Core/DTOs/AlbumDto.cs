namespace MusicCatalogService.Core.DTOs;

public class AlbumDto
{
    public string SpotifyId { get; set; }
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public string ImageUrl { get; set; }
    public string ReleaseDate { get; set; }
    public string AlbumType { get; set; }
}
