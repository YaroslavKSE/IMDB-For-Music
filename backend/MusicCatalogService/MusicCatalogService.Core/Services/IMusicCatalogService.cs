using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Services;

public interface IMusicCatalogService
{
    Task<CatalogItemDto> GetAlbumAsync(string spotifyId);
    Task<CatalogItemDto> GetTrackAsync(string spotifyId);
    Task<CatalogItemDto> GetArtistAsync(string spotifyId);
    Task<SearchResultDto> SearchAsync(string query, string type, int limit = 20, int offset = 0);
    Task<NewReleasesDto> GetNewReleasesAsync(int limit = 20, int offset = 0);
}