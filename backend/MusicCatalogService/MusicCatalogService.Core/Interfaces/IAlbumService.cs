using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface IAlbumService
{
    // Get album from Spotify and cache
    Task<AlbumDetailDto> GetAlbumAsync(string spotifyId);
    
    // Get album from internal database 
    Task<AlbumDetailDto> GetAlbumByCatalogIdAsync(Guid catalogId);
    
    // Save album into internal database 
    Task<AlbumDetailDto> SaveAlbumAsync(string spotifyId);

    // New method to get multiple albums with simplified overview
    Task<MultipleAlbumsOverviewDto> GetMultipleAlbumsOverviewAsync(IEnumerable<string> spotifyIds);
    // Add this method to MusicCatalogService.Core.Interfaces.IAlbumService
    Task<AlbumTracksResultDto> GetAlbumTracksAsync(string spotifyId, int limit = 20, int offset = 0, string? market = null);
}