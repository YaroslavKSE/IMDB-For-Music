using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Core.Interfaces;

public interface ICatalogRepository
{
    Task<CatalogItem> GetBySpotifyIdAsync(string spotifyId, string type);
    Task AddOrUpdateAsync(CatalogItem catalogItem);
    Task<IEnumerable<CatalogItem>> GetBatchBySpotifyIdsAsync(IEnumerable<string> spotifyIds, string type);
}