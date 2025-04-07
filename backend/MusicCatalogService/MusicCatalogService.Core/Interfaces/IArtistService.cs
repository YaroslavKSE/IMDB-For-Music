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
}