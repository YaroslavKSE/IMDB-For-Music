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
    private readonly IMongoCollection<Artist> _artists;
    private readonly ILogger<MongoCatalogRepository> _logger;

    public MongoCatalogRepository(
        IOptions<MongoDbSettings> dbSettings,
        ILogger<MongoCatalogRepository> logger)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

        // Initialize collections
        _albums = database.GetCollection<Album>(dbSettings.Value.AlbumsCollectionName);
        _tracks = database.GetCollection<Track>(dbSettings.Value.TracksCollectionName);
        _artists = database.GetCollection<Artist>(dbSettings.Value.ArtistsCollectionName);
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

        // Create index on SpotifyId for Artists collection
        var artistIndexKeysDefinition = Builders<Artist>.IndexKeys
            .Ascending(artist => artist.SpotifyId);

        await _artists.Indexes.CreateOneAsync(
            new CreateIndexModel<Artist>(artistIndexKeysDefinition, new CreateIndexOptions {Unique = true}));

        // Create indexes on CacheExpiresAt for all collections to help with cache cleanup
        await _albums.Indexes.CreateOneAsync(
            new CreateIndexModel<Album>(
                Builders<Album>.IndexKeys.Ascending(item => item.CacheExpiresAt)));

        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Ascending(item => item.CacheExpiresAt)));

        await _artists.Indexes.CreateOneAsync(
            new CreateIndexModel<Artist>(
                Builders<Artist>.IndexKeys.Ascending(item => item.CacheExpiresAt)));
        // Popularity index for sorting operations
        await _albums.Indexes.CreateOneAsync(
            new CreateIndexModel<Album>(
                Builders<Album>.IndexKeys.Descending(item => item.Popularity)));

        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Descending(item => item.Popularity)));

        await _artists.Indexes.CreateOneAsync(
            new CreateIndexModel<Artist>(
                Builders<Artist>.IndexKeys.Descending(item => item.Popularity)));
        // Release date index for albums
        await _albums.Indexes.CreateOneAsync(
            new CreateIndexModel<Album>(
                Builders<Album>.IndexKeys.Descending(item => item.ReleaseDate)));

        // Create index on AlbumId for Tracks to quickly find tracks belonging to an album
        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Ascending(track => track.AlbumId)));

        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys.Ascending(track => track.Isrc),
                new CreateIndexOptions {Background = true, Sparse = true}));
        // Track number index for tracks (to sort tracks within an album)
        await _tracks.Indexes.CreateOneAsync(
            new CreateIndexModel<Track>(
                Builders<Track>.IndexKeys
                    .Ascending(track => track.AlbumId)
                    .Ascending(track => track.DiscNumber)
                    .Ascending(track => track.TrackNumber)));
    }

    // Existing Album methods by Spotify ID
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

    // Save album permanently
    public async Task SaveAlbumAsync(Album album)
    {
        try
        {
            // For a permanent save, set the cache to a day
            album.CacheExpiresAt = DateTime.UtcNow.AddDays(1);

            var filter = Builders<Album>.Filter.Eq(a => a.SpotifyId, album.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _albums.ReplaceOneAsync(filter, album, options);

            _logger.LogInformation("Album with SpotifyId {SpotifyId} permanently saved", album.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently saving album with SpotifyId {SpotifyId}", album.SpotifyId);
            throw;
        }
    }

    public async Task<Album> GetAlbumByIdAsync(Guid catalogId)
    {
        try
        {
            var filter = Builders<Album>.Filter.Eq(album => album.Id, catalogId);
            return await _albums.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving album with catalog ID {CatalogId}", catalogId);
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

    // New method - Save track permanently
    public async Task SaveTrackAsync(Track track)
    {
        try
        {
            // For a permanent save, ensure the result for a day
            track.CacheExpiresAt = DateTime.UtcNow.AddDays(1);

            var filter = Builders<Track>.Filter.Eq(t => t.SpotifyId, track.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _tracks.ReplaceOneAsync(filter, track, options);

            _logger.LogInformation("Track with SpotifyId {SpotifyId} permanently saved", track.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently saving track with SpotifyId {SpotifyId}", track.SpotifyId);
            throw;
        }
    }

    // New method - Get track by catalog ID (Guid)
    public async Task<Track> GetTrackByIdAsync(Guid catalogId)
    {
        try
        {
            var filter = Builders<Track>.Filter.Eq(track => track.Id, catalogId);
            return await _tracks.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving track with catalog ID {CatalogId}", catalogId);
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

    // Artist methods
    public async Task<Artist> GetArtistBySpotifyIdAsync(string spotifyId)
    {
        try
        {
            var filter = Builders<Artist>.Filter.Eq(artist => artist.SpotifyId, spotifyId);
            return await _artists.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artist with SpotifyId {SpotifyId}", spotifyId);
            throw;
        }
    }

    public async Task AddOrUpdateArtistAsync(Artist artist)
    {
        try
        {
            var filter = Builders<Artist>.Filter.Eq(a => a.SpotifyId, artist.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _artists.ReplaceOneAsync(filter, artist, options);

            _logger.LogInformation("Artist with SpotifyId {SpotifyId} saved successfully", artist.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving artist with SpotifyId {SpotifyId}", artist.SpotifyId);
            throw;
        }
    }

    public async Task<IEnumerable<Artist>> GetBatchArtistsBySpotifyIdsAsync(IEnumerable<string> spotifyIds)
    {
        try
        {
            var filter = Builders<Artist>.Filter.In(artist => artist.SpotifyId, spotifyIds);
            return await _artists.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch of artists");
            throw;
        }
    }

    public async Task<Artist> GetArtistByIdAsync(Guid catalogId)
    {
        try
        {
            var filter = Builders<Artist>.Filter.Eq(artist => artist.Id, catalogId);
            return await _artists.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artist with catalog ID {CatalogId}", catalogId);
            throw;
        }
    }

    public async Task SaveArtistAsync(Artist artist)
    {
        try
        {
            // For a permanent save, cache the result for a day 
            artist.CacheExpiresAt = DateTime.UtcNow.AddDays(1);

            var filter = Builders<Artist>.Filter.Eq(a => a.SpotifyId, artist.SpotifyId);
            var options = new ReplaceOptions {IsUpsert = true};

            await _artists.ReplaceOneAsync(filter, artist, options);

            _logger.LogInformation("Artist with SpotifyId {SpotifyId} permanently saved", artist.SpotifyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently saving artist with SpotifyId {SpotifyId}", artist.SpotifyId);
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
        else if (typeof(T) == typeof(Artist))
        {
            var artist = await GetArtistBySpotifyIdAsync(spotifyId);
            return artist as T;
        }

        throw new ArgumentException($"Type {typeof(T).Name} is not supported.");
    }

    public async Task<T> GetByIdAsync<T>(Guid catalogId) where T : CatalogItemBase
    {
        if (typeof(T) == typeof(Album))
        {
            var album = await GetAlbumByIdAsync(catalogId);
            return album as T;
        }
        else if (typeof(T) == typeof(Track))
        {
            var track = await GetTrackByIdAsync(catalogId);
            return track as T;
        }
        else if (typeof(T) == typeof(Artist))
        {
            var artist = await GetArtistByIdAsync(catalogId);
            return artist as T;
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

            // Delete expired artists
            var artistsFilter = Builders<Artist>.Filter.Lt(a => a.CacheExpiresAt, currentTime);
            var artistsResult = await _artists.DeleteManyAsync(artistsFilter);

            _logger.LogInformation(
                "Cleaned up expired items: {AlbumCount} albums, {TrackCount} tracks, and {ArtistCount} artists deleted",
                albumsResult.DeletedCount, tracksResult.DeletedCount, artistsResult.DeletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache items");
            throw;
        }
    }
}