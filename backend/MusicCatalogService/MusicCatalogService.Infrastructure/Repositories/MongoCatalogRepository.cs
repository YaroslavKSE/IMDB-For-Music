using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Infrastructure.Configuration;

namespace MusicCatalogService.Infrastructure.Repositories;

public class MongoCatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<CatalogItem> _catalogItems;
    private readonly ILogger<MongoCatalogRepository> _logger;

    public MongoCatalogRepository(
        IOptions<MongoDbSettings> dbSettings,
        ILogger<MongoCatalogRepository> logger)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _catalogItems = database.GetCollection<CatalogItem>(dbSettings.Value.CatalogItemsCollectionName);
        _logger = logger;
        
        // Create indexes (if needed)
        CreateIndexesAsync().GetAwaiter().GetResult();
    }
    
    private async Task CreateIndexesAsync()
    {
        // Create a compound index on SpotifyId and Type for efficient lookups
        var indexKeysDefinition = Builders<CatalogItem>.IndexKeys
            .Ascending(item => item.SpotifyId)
            .Ascending(item => item.Type);
        
        await _catalogItems.Indexes.CreateOneAsync(
            new CreateIndexModel<CatalogItem>(indexKeysDefinition, new CreateIndexOptions { Unique = true }));
        
        // Index for CacheExpiresAt to help with cache cleanup tasks
        await _catalogItems.Indexes.CreateOneAsync(
            new CreateIndexModel<CatalogItem>(
                Builders<CatalogItem>.IndexKeys.Ascending(item => item.CacheExpiresAt)));
    }

    public async Task<CatalogItem> GetBySpotifyIdAsync(string spotifyId, string type)
    {
        try
        {
            var filter = Builders<CatalogItem>.Filter.And(
                Builders<CatalogItem>.Filter.Eq(item => item.SpotifyId, spotifyId),
                Builders<CatalogItem>.Filter.Eq(item => item.Type, type));
            
            return await _catalogItems.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving catalog item with SpotifyId {SpotifyId} and type {Type}", 
                spotifyId, type);
            throw;
        }
    }

    public async Task AddOrUpdateAsync(CatalogItem catalogItem)
    {
        try
        {
            var filter = Builders<CatalogItem>.Filter.And(
                Builders<CatalogItem>.Filter.Eq(item => item.SpotifyId, catalogItem.SpotifyId),
                Builders<CatalogItem>.Filter.Eq(item => item.Type, catalogItem.Type));
            
            var options = new ReplaceOptions { IsUpsert = true };
            
            await _catalogItems.ReplaceOneAsync(filter, catalogItem, options);
            
            _logger.LogInformation("Catalog item with SpotifyId {SpotifyId} and type {Type} saved successfully", 
                catalogItem.SpotifyId, catalogItem.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving catalog item with SpotifyId {SpotifyId} and type {Type}", 
                catalogItem.SpotifyId, catalogItem.Type);
            throw;
        }
    }

    public async Task<IEnumerable<CatalogItem>> GetBatchBySpotifyIdsAsync(IEnumerable<string> spotifyIds, string type)
    {
        try
        {
            var filter = Builders<CatalogItem>.Filter.And(
                Builders<CatalogItem>.Filter.In(item => item.SpotifyId, spotifyIds),
                Builders<CatalogItem>.Filter.Eq(item => item.Type, type));
            
            return await _catalogItems.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving catalog items batch with type {Type}", type);
            throw;
        }
    }
}