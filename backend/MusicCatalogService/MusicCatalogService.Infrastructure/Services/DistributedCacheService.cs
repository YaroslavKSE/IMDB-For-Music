using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MusicCatalogService.Core.Interfaces;

namespace MusicCatalogService.Infrastructure.Services;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistributedCacheService> _logger;
    
    public DistributedCacheService(
        IDistributedCache distributedCache,
        ILogger<DistributedCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }
    
    public async Task<T> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(cachedData))
            {
                return null;
            }
            
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data from cache for key {Key}", key);
            return null;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }
            
            var serializedData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data in cache for key {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key {Key}", key);
        }
    }
}