using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Infrastructure.Configuration;

namespace MusicCatalogService.Infrastructure.Repositories;

public class MongoCatalogRepository : ICatalogRepository
{
    private readonly IMongoCollection<Album> _albums;
    private readonly IMongoCollection<Track> _tracks;
    private readonly ILogger<MongoCatalogRepository> _logger;

    public MongoCatalogRepository(
        IOptions<MongoDbSettings> dbSettings,
        ILogger<MongoCatalogRepository> logger)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

        // Initialize separate collections
        _albums = database.GetCollection<Album>(dbSettings.Value.AlbumsCollectionName);
        _tracks = database.GetCollection<Track>(dbSettings.Value.TracksCollectionName);
        _logger = logger;

        // Create indexes
        CreateIndexesAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesAsync()
    {
        // Create index on SpotifyId for Albums collection
        var albumIndexKeysDefinition = Builders<Album>.IndexKeys
            .Ascending(album => album.SpotifyId);

        await _albums.Indexes.CreateOneAsync(
            new CreateIndexModel<Album>(albumIndexKeysDefinition, new CreateIndexOptions {Unique = true}));

        // Create index on SpotifyId for Tracks collection
        var trackIndexKeysDefinition = Builders<Track>.IndexKeys
            .Ascending(track => track.SpotifyId);

        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(trackIndexKeysDefinition, new CreateIndexOptions {Unique = true}));

        // Create index on CacheExpiresAt for both collections to help with cache cleanup
        await _albums.Indexes.CreateOneAsync(
            new CreateIndexModel<Album>(
                Builders<Album>.IndexKeys.Ascending(item => item.CacheExpiresAt)));

        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Ascending(item => item.CacheExpiresAt)));

        // Create index on AlbumId for Tracks to quickly find tracks belonging to an album
        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Ascending(track => track.AlbumId)));
    }

    // Album methods
    public async Task<Album> GetAlbumBySpotifyIdAsync(string spotifyId)
    {
        try
        {
            var filter = Builders<Album>.Filter.Eq(album => album.SpotifyId, spotifyId);
            return await _albums.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving album with SpotifyId {SpotifyId}", spotifyId);
            throw;
        }
    }

    public async Task AddOrUpdateAlbumAsync(Album album)
    {
        try
        {
            var filter = Builders<Album>.Filter.Eq(a => a.SpotifyId, album.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _albums.ReplaceOneAsync(filter, album, options);

            _logger.LogInformation("Album with SpotifyId {SpotifyId} saved successfully", album.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving album with SpotifyId {SpotifyId}", album.SpotifyId);
            throw;
        }
    }

    public async Task<IEnumerable<Album>> GetBatchAlbumsBySpotifyIdsAsync(IEnumerable<string> spotifyIds)
    {
        try
        {
            var filter = Builders<Album>.Filter.In(album => album.SpotifyId, spotifyIds);
            return await _albums.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch of albums");
            throw;
        }
    }

    // Track methods
    public async Task<Track> GetTrackBySpotifyIdAsync(string spotifyId)
    {
        try
        {
            var filter = Builders<Track>.Filter.Eq(track => track.SpotifyId, spotifyId);
            return await _tracks.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving track with SpotifyId {SpotifyId}", spotifyId);
            throw;
        }
    }

    public async Task AddOrUpdateTrackAsync(Track track)
    {
        try
        {
            var filter = Builders<Track>.Filter.Eq(t => t.SpotifyId, track.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _tracks.ReplaceOneAsync(filter, track, options);

            _logger.LogInformation("Track with SpotifyId {SpotifyId} saved successfully", track.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving track with SpotifyId {SpotifyId}", track.SpotifyId);
            throw;
        }
    }

    public async Task<IEnumerable<Track>> GetBatchTracksBySpotifyIdsAsync(IEnumerable<string> spotifyIds)
    {
        try
        {
            var filter = Builders<Track>.Filter.In(track => track.SpotifyId, spotifyIds);
            return await _tracks.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch of tracks");
            throw;
        }
    }

    // Generic method implementation
    public async Task<T> GetBySpotifyIdAsync<T>(string spotifyId) where T : CatalogItemBase
    {
        if (typeof(T) == typeof(Album))
        {
            var album = await GetAlbumBySpotifyIdAsync(spotifyId);
            return album as T;
        }
        else if (typeof(T) == typeof(Track))
        {
            var track = await GetTrackBySpotifyIdAsync(spotifyId);
            return track as T;
        }

        throw new ArgumentException($"Type {typeof(T).Name} is not supported.");
    }

    // Cleanup expired items
    public async Task CleanupExpiredItemsAsync()
    {
        try
        {
            var currentTime = DateTime.UtcNow;

            // Delete expired albums
            var albumsFilter = Builders<Album>.Filter.Lt(a => a.CacheExpiresAt, currentTime);
            var albumsResult = await _albums.DeleteManyAsync(albumsFilter);

            // Delete expired tracks
            var tracksFilter = Builders<Track>.Filter.Lt(t => t.CacheExpiresAt, currentTime);
            var tracksResult = await _tracks.DeleteManyAsync(tracksFilter);

            _logger.LogInformation(
                "Cleaned up expired items: {AlbumCount} albums and {TrackCount} tracks deleted",
                albumsResult.DeletedCount, tracksResult.DeletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache items");
            throw;
        }
    }
}