using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Core.Interfaces;

public interface ICatalogRepository
{
    // Album methods
    Task<Album> GetAlbumBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateAlbumAsync(Album album);
    Task<IEnumerable<Album>> GetBatchAlbumsBySpotifyIdsAsync(IEnumerable<string> spotifyIds);
    Task<Album> GetAlbumByIdAsync(Guid catalogId);
    Task SaveAlbumAsync(Album album);
    
    // Track methods
    Task<Track> GetTrackBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateTrackAsync(Track track);
    Task<IEnumerable<Track>> GetBatchTracksBySpotifyIdsAsync(IEnumerable<string> spotifyIds);
    Task<Track> GetTrackByIdAsync(Guid catalogId);
    Task SaveTrackAsync(Track track);
    
    // Artist methods
    Task<Artist> GetArtistBySpotifyIdAsync(string spotifyId);
    Task AddOrUpdateArtistAsync(Artist artist);
    Task<IEnumerable<Artist>> GetBatchArtistsBySpotifyIdsAsync(IEnumerable<string> spotifyIds);
    Task<Artist> GetArtistByIdAsync(Guid catalogId);
    Task SaveArtistAsync(Artist artist);
    
    // Search methods
    Task<(List<Album>, int)> SearchAlbumsAsync(string query, int limit, int offset);
    Task<(List<Artist>, int)> SearchArtistsAsync(string query, int limit, int offset);
    Task<(List<Track>, int)> SearchTracksAsync(string query, int limit, int offset);
    
    // Generic method updated to support both lookup types
    Task<T> GetBySpotifyIdAsync<T>(string spotifyId) where T : CatalogItemBase;
    Task<T> GetByIdAsync<T>(Guid catalogId) where T : CatalogItemBase;
    
    // Cleanup method
    Task CleanupExpiredItemsAsync();
}