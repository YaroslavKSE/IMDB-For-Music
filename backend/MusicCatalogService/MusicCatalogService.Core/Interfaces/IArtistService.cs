using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface IArtistService
{
    // Get artist from Spotify and cache
    Task<ArtistDetailDto> GetArtistAsync(string spotifyId);
    
    // Get artist from internal database
    Task<ArtistDetailDto> GetArtistByCatalogIdAsync(Guid catalogId);
    
    // Save artist into internal database
    Task<ArtistDetailDto> SaveArtistAsync(string spotifyId);

    // Add these to MusicCatalogService.Core.Interfaces.IArtistService

    // Get artist albums
    Task<ArtistAlbumsResultDto> GetArtistAlbumsAsync(string spotifyId, int limit = 20, int offset = 0, string? market = null, string? includeGroups = "album");
    // Get artist top tracks
    Task<ArtistTopTracksResultDto> GetArtistTopTracksAsync(string spotifyId, string? market = null);
}