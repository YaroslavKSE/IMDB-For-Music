using MusicCatalogService.Core.Models.Spotify;

namespace MusicCatalogService.Core.Interfaces;

public interface ISpotifyApiClient
{
    Task<SpotifyAlbumResponse?> GetAlbumAsync(string albumId);
    Task<SpotifyTrackResponse?> GetTrackAsync(string trackId);
    Task<SpotifyArtistResponse?> GetArtistAsync(string artistId);
    Task<SpotifySearchResponse?> SearchAsync(string query, string type, int limit = 20, int offset = 0);
    Task<SpotifyNewReleasesResponse?> GetNewReleasesAsync(int limit = 20, int offset = 0);
}