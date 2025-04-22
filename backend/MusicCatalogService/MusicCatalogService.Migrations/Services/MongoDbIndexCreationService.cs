using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Infrastructure.Configuration;

namespace MusicCatalogService.Migrations.Services;

public class MongoDbIndexCreationService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbIndexCreationService> _logger;
    private readonly MongoDbSettings _settings;
    private readonly IMongoCollection<Album> _albums;
    private readonly IMongoCollection<Track> _tracks;
    private readonly IMongoCollection<Artist> _artists;

    public MongoDbIndexCreationService(
        IOptions<MongoDbSettings> settings,
        ILogger<MongoDbIndexCreationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);

        _albums = _database.GetCollection<Album>(_settings.AlbumsCollectionName);
        _tracks = _database.GetCollection<Track>(_settings.TracksCollectionName);
        _artists = _database.GetCollection<Artist>(_settings.ArtistsCollectionName);
    }

    public async Task CreateIndexesAsync()
    {
        try
        {
            _logger.LogInformation("Creating and updating indexes in MongoDB collections");

            // Create index on SpotifyId for Albums collection
            var albumIndexKeysDefinition = Builders<Album>.IndexKeys
                .Ascending(album => album.SpotifyId);

            await _albums.Indexes.CreateOneAsync(
                new CreateIndexModel<Album>(albumIndexKeysDefinition,
                    new CreateIndexOptions {Unique = true, Background = true}));

            // Create index on SpotifyId for Tracks collection
            var trackIndexKeysDefinition = Builders<Track>.IndexKeys
                .Ascending(track => track.SpotifyId);

            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(trackIndexKeysDefinition,
                    new CreateIndexOptions {Unique = true, Background = true}));

            // Create index on SpotifyId for Artists collection
            var artistIndexKeysDefinition = Builders<Artist>.IndexKeys
                .Ascending(artist => artist.SpotifyId);

            await _artists.Indexes.CreateOneAsync(
                new CreateIndexModel<Artist>(artistIndexKeysDefinition,
                    new CreateIndexOptions {Unique = true, Background = true}));

            // Create indexes on CacheExpiresAt for all collections to help with cache cleanup
            await _albums.Indexes.CreateOneAsync(
                new CreateIndexModel<Album>(
                    Builders<Album>.IndexKeys.Ascending(item => item.CacheExpiresAt),
                    new CreateIndexOptions {Background = true}));

            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(
                    Builders<Track>.IndexKeys.Ascending(item => item.CacheExpiresAt),
                    new CreateIndexOptions {Background = true}));

            await _artists.Indexes.CreateOneAsync(
                new CreateIndexModel<Artist>(
                    Builders<Artist>.IndexKeys.Ascending(item => item.CacheExpiresAt),
                    new CreateIndexOptions {Background = true}));

            // Create index on AlbumId for Tracks to quickly find tracks belonging to an album
            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(
                    Builders<Track>.IndexKeys.Ascending(track => track.AlbumId),
                    new CreateIndexOptions {Background = true}));

            // Create index on ISRC for Tracks
            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(
                    Builders<Track>.IndexKeys.Ascending(track => track.Isrc),
                    new CreateIndexOptions {Background = true, Sparse = true}));

            // Create index on Popularity for sorting
            await _albums.Indexes.CreateOneAsync(
                new CreateIndexModel<Album>(
                    Builders<Album>.IndexKeys.Descending(item => item.Popularity),
                    new CreateIndexOptions {Background = true}));

            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(
                    Builders<Track>.IndexKeys.Descending(item => item.Popularity),
                    new CreateIndexOptions {Background = true}));

            await _artists.Indexes.CreateOneAsync(
                new CreateIndexModel<Artist>(
                    Builders<Artist>.IndexKeys.Descending(item => item.Popularity),
                    new CreateIndexOptions {Background = true}));

            // Create index on ReleaseDate for albums
            await _albums.Indexes.CreateOneAsync(
                new CreateIndexModel<Album>(
                    Builders<Album>.IndexKeys.Descending(item => item.ReleaseDate),
                    new CreateIndexOptions {Background = true}));

            // Create compound index for track ordering within albums
            await _tracks.Indexes.CreateOneAsync(
                new CreateIndexModel<Track>(
                    Builders<Track>.IndexKeys
                        .Ascending(track => track.AlbumId)
                        .Ascending(track => track.DiscNumber)
                        .Ascending(track => track.TrackNumber),
                    new CreateIndexOptions {Background = true}));

            _logger.LogInformation("Successfully created all indexes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MongoDB indexes");
            throw;
        }
    }
}