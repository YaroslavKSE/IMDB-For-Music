namespace MusicCatalogService.Core.Models;

public class Album : CatalogItemBase
{
    // Album-specific fields
    public string ReleaseDate { get; set; }

    public string ReleaseDatePrecision { get; set; }

    public string AlbumType { get; set; } // album, single, compilation

    public int? TotalTracks { get; set; }

    public string Label { get; set; }

    public string Copyright { get; set; }

    // External URLs
    public string SpotifyUrl { get; set; }

    // List of simplified artists
    public List<SimplifiedArtist> Artists { get; set; } = new();

    // Optional genre information
    public List<string> Genres { get; set; } = new();
}