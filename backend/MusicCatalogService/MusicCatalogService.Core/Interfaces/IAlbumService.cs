using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface IAlbumService
{
    Task<AlbumDetailDto> GetAlbumAsync(string spotifyId);
}