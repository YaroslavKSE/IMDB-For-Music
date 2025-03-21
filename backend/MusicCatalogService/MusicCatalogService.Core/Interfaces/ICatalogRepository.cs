using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Core.Interfaces;

public interface ICatalogRepository
{
    // Album methods
    Task<Album> GetAlbumBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateAlbumAsync(Album album);
    Task<IEnumerable<Album>> GetBatchAlbumsBySpotifyIdsAsync(IEnumerable<string> spotifyIds);

    // Track methods
    Task<Track> GetTrackBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateTrackAsync(Track track);
    Task<IEnumerable<Track>> GetBatchTracksBySpotifyIdsAsync(IEnumerable<string> spotifyIds);

    // Generic method to get either an album or track by Spotify ID
    Task<T> GetBySpotifyIdAsync<T>(string spotifyId) where T : CatalogItemBase;

    // Clean up expired cache items (optional, for maintenance)
    Task CleanupExpiredItemsAsync();
}