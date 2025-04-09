using System.Text.Json.Serialization;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Models.Spotify;

public class SpotifyMultipleAlbumsResponse
{
    [JsonPropertyName("albums")]
    public List<SpotifyAlbumResponse> Albums { get; set; }
}

public class SpotifyMultipleTracksResponse
{
    [JsonPropertyName("tracks")]
    public List<SpotifyTrackResponse> Tracks { get; set; }
}