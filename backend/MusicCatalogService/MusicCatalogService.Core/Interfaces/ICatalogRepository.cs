using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Core.Interfaces;

public interface ICatalogRepository
{
    // Existing methods for Spotify ID lookups
    Task<Album> GetAlbumBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateAlbumAsync(Album album);
    Task<IEnumerable<Album>> GetBatchAlbumsBySpotifyIdsAsync(IEnumerable<string> spotifyIds);
    
    Task<Track> GetTrackBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateTrackAsync(Track track);
    Task<IEnumerable<Track>> GetBatchTracksBySpotifyIdsAsync(IEnumerable<string> spotifyIds);
    
    // Get methods for retrieving from MongoDb using Catalog ID lookups
    Task<Album> GetAlbumByIdAsync(Guid catalogId);
    Task<Track> GetTrackByIdAsync(Guid catalogId);
    
    // Generic method updated to support both lookup types
    Task<T> GetBySpotifyIdAsync<T>(string spotifyId) where T : CatalogItemBase;
    Task<T> GetByIdAsync<T>(Guid catalogId) where T : CatalogItemBase;
    
    // Save method (ensures item is explicitly stored permanently)
    Task SaveAlbumAsync(Album album);
    Task SaveTrackAsync(Track track);
    
    // Cleanup method (existing)
    Task CleanupExpiredItemsAsync();
}