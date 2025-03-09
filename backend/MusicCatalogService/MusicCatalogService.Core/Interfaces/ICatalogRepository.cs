using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Core.Interfaces;

public interface ICatalogRepository
{
    Task<CatalogItem> GetByIdAsync(Guid id);
    Task<CatalogItem> GetBySpotifyIdAsync(string spotifyId, string type);
    Task<List<CatalogItem>> GetRecentlyAccessedAsync(int limit);
    Task<List<CatalogItem>> GetPopularAsync(int limit);
    Task AddAsync(CatalogItem catalogItem);
    Task UpdateAsync(CatalogItem catalogItem);
    Task<List<CatalogItem>> GetExpiredCacheItemsAsync();
    Task<List<CatalogItem>> GetByIdsAsync(List<Guid> ids);
}