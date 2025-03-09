using Microsoft.EntityFrameworkCore;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Infrastructure.Data;

public class CatalogRepository : ICatalogRepository
{
    private readonly MusicCatalogDbContext _dbContext;

    public CatalogRepository(MusicCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CatalogItem> GetByIdAsync(Guid id)
    {
        return await _dbContext.CatalogItems.FindAsync(id);
    }

    public async Task<CatalogItem> GetBySpotifyIdAsync(string spotifyId, string type)
    {
        return await _dbContext.CatalogItems
            .FirstOrDefaultAsync(ci => ci.SpotifyId == spotifyId && ci.Type == type);
    }

    public async Task<List<CatalogItem>> GetRecentlyAccessedAsync(int limit)
    {
        return await _dbContext.CatalogItems
            .OrderByDescending(ci => ci.LastAccessed)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<CatalogItem>> GetPopularAsync(int limit)
    {
        return await _dbContext.CatalogItems
            .Where(ci => ci.Popularity.HasValue)
            .OrderByDescending(ci => ci.Popularity)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(CatalogItem catalogItem)
    {
        await _dbContext.CatalogItems.AddAsync(catalogItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(CatalogItem catalogItem)
    {
        _dbContext.CatalogItems.Update(catalogItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CatalogItem>> GetExpiredCacheItemsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.CatalogItems
            .Where(ci => ci.CacheExpiresAt < now)
            .ToListAsync();
    }

    public async Task<List<CatalogItem>> GetByIdsAsync(List<Guid> ids)
    {
        return await _dbContext.CatalogItems
            .Where(ci => ids.Contains(ci.Id))
            .ToListAsync();
    }
}