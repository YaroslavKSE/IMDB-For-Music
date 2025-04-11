using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Interfaces;

public interface ISpotifyApiClient
{
    Task<SpotifyAlbumResponse?> GetAlbumAsync(string albumId);
    Task<SpotifyTrackResponse?> GetTrackAsync(string trackId);
    Task<SpotifyArtistResponse?> GetArtistAsync(string artistId);
    Task<SpotifySearchResponse?> SearchAsync(string query, string type, int limit = 20, int offset = 0);
    Task<SpotifyNewReleasesResponse?> GetNewReleasesAsync(int limit = 20, int offset = 0);

    // New batch retrieval methods
    Task<SpotifyMultipleAlbumsResponse?> GetMultipleAlbumsAsync(IEnumerable<string> albumIds);
    Task<SpotifyMultipleTracksResponse?> GetMultipleTracksAsync(IEnumerable<string> trackIds);
    Task<SpotifyArtistAlbumsResponse?> GetArtistAlbumsAsync(string artistId, int limit = 20, int offset = 0, string? market = null);
    Task<SpotifyArtistTopTracksResponse?> GetArtistTopTracksAsync(string artistId, string? market = null);
}