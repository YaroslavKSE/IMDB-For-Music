using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Spotify;
using MusicCatalogService.Infrastructure.Configuration;

namespace MusicCatalogService.Migrations.Services;

public class MongoDbMigrationService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbMigrationService> _logger;
    private readonly MongoDbSettings _settings;

    public MongoDbMigrationService(
        IOptions<MongoDbSettings> settings,
        ILogger<MongoDbMigrationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting MongoDB migration process...");

        await MigrateAlbumsAsync(cancellationToken);
        await MigrateTracksAsync(cancellationToken);
        await MigrateArtistsAsync(cancellationToken);

        _logger.LogInformation("MongoDB migration completed successfully.");
    }

    private async Task MigrateAlbumsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Migrating Albums collection...");

        // Get collections - use only BsonDocument for migration
        var albumsCollection = _database.GetCollection<BsonDocument>(_settings.AlbumsCollectionName);

        // Find all documents with RawData field
        var filter = Builders<BsonDocument>.Filter.Exists("RawData", true);
        var count = await albumsCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        _logger.LogInformation("Found {Count} albums with RawData to migrate", count);

        if (count == 0)
        {
            _logger.LogInformation("No albums require migration. Skipping.");
            return;
        }

        var cursor = await albumsCollection.FindAsync(filter,
            new FindOptions<BsonDocument, BsonDocument> {BatchSize = 100},
            cancellationToken);

        var processed = 0;

        while (await cursor.MoveNextAsync(cancellationToken))
            foreach (var doc in cursor.Current)
                try
                {
                    if (doc.Contains("RawData") && !doc["RawData"].IsBsonNull)
                    {
                        var rawData = doc["RawData"].AsString;

                        // Parse the raw data
                        try
                        {
                            var spotifyAlbum = JsonSerializer.Deserialize<SpotifyAlbumResponse>(
                                rawData,
                                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

                            if (spotifyAlbum != null)
                            {
                                // Create update for needed fields
                                var update = Builders<BsonDocument>.Update
                                    .Set("ThumbnailUrl", GetOptimalImage(spotifyAlbum.Images))
                                    .Set("ReleaseDatePrecision", spotifyAlbum.ReleaseDatePrecision)
                                    .Set("Label", spotifyAlbum.Label)
                                    .Set("Copyright", spotifyAlbum.Copyright)
                                    .Unset("RawData");

                                // Update artists array and track IDs if needed
                                // Apply update
                                await albumsCollection.UpdateOneAsync(
                                    Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                                    update,
                                    cancellationToken: cancellationToken);

                                processed++;
                                if (processed % 50 == 0)
                                    _logger.LogInformation("Processed {Count}/{Total} albums", processed, count);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing raw data for album {Id}", doc["_id"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing album document {Id}", doc["_id"]);
                }

        _logger.LogInformation("Completed album migration: {Count}/{Total} processed", processed, count);
    }

    private async Task MigrateTracksAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Migrating Tracks collection...");

        // Get collection using BsonDocument (not Track class)
        var tracksCollection = _database.GetCollection<BsonDocument>(_settings.TracksCollectionName);

        // Find all documents with RawData field
        var filter = Builders<BsonDocument>.Filter.Exists("RawData", true);
        var count = await tracksCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        _logger.LogInformation("Found {Count} tracks with RawData to migrate", count);

        if (count == 0)
        {
            _logger.LogInformation("No tracks require migration. Skipping.");
            return;
        }

        // Process in batches for better performance
        const int batchSize = 100;
        var processed = 0;

        var cursor = await tracksCollection.FindAsync(filter,
            new FindOptions<BsonDocument, BsonDocument> {BatchSize = batchSize},
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
            foreach (var doc in cursor.Current)
                try
                {
                    if (doc.Contains("RawData") && !doc["RawData"].IsBsonNull)
                    {
                        var rawData = doc["RawData"].AsString;

                        try
                        {
                            var spotifyTrack = JsonSerializer.Deserialize<SpotifyTrackResponse>(
                                rawData,
                                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

                            if (spotifyTrack != null)
                            {
                                // Create update for needed fields
                                var update = Builders<BsonDocument>.Update
                                    .Set("ThumbnailUrl", GetOptimalImage(spotifyTrack.Album?.Images))
                                    .Set("PreviewUrl", spotifyTrack.PreviewUrl)
                                    .Set("TrackNumber", spotifyTrack.TrackNumber)
                                    .Set("DiscNumber", spotifyTrack.DiscNumber)
                                    .Set("Isrc", spotifyTrack.ExternalIds?.Isrc)
                                    .Unset("RawData");

                                // Convert artists to BSON array and update
                                if (spotifyTrack.Artists != null && spotifyTrack.Artists.Any())
                                {
                                    var artistsBson = new BsonArray();
                                    foreach (var artist in spotifyTrack.Artists)
                                        artistsBson.Add(new BsonDocument
                                        {
                                            {"Id", artist.Id},
                                            {"Name", artist.Name},
                                            {"SpotifyUrl", artist.ExternalUrls?.Spotify}
                                        });
                                    update = update.Set("Artists", artistsBson);
                                }

                                // Apply update
                                await tracksCollection.UpdateOneAsync(
                                    Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                                    update,
                                    cancellationToken: cancellationToken);

                                processed++;
                                if (processed % 50 == 0)
                                    _logger.LogInformation("Processed {Count}/{Total} tracks", processed, count);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing raw data for track {Id}", doc["_id"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing track document {Id}", doc["_id"]);
                }

        _logger.LogInformation("Completed track migration: {Count}/{Total} processed", processed, count);
    }

    private async Task MigrateArtistsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Migrating Artists collection...");

        // Get collection using BsonDocument
        var artistsCollection = _database.GetCollection<BsonDocument>(_settings.ArtistsCollectionName);

        // Find all documents with RawData field
        var filter = Builders<BsonDocument>.Filter.Exists("RawData", true);
        var count = await artistsCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        _logger.LogInformation("Found {Count} artists with RawData to migrate", count);

        if (count == 0)
        {
            _logger.LogInformation("No artists require migration. Skipping.");
            return;
        }

        // Process in batches for better performance
        const int batchSize = 100;
        var processed = 0;

        var cursor = await artistsCollection.FindAsync(filter,
            new FindOptions<BsonDocument, BsonDocument> {BatchSize = batchSize},
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
            foreach (var doc in cursor.Current)
                try
                {
                    if (doc.Contains("RawData") && !doc["RawData"].IsBsonNull)
                    {
                        var rawData = doc["RawData"].AsString;

                        try
                        {
                            var spotifyArtist = JsonSerializer.Deserialize<SpotifyArtistResponse>(
                                rawData,
                                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

                            if (spotifyArtist != null)
                            {
                                // Create update for needed fields
                                var update = Builders<BsonDocument>.Update
                                    .Set("ThumbnailUrl", GetOptimalImage(spotifyArtist.Images))
                                    .Set("FollowersCount", spotifyArtist.FollowersCount)
                                    .Set("SpotifyUrl", spotifyArtist.ExternalUrls?.Spotify)
                                    .Unset("RawData");

                                // Handle genres if they exist
                                if (spotifyArtist.Genres != null && spotifyArtist.Genres.Any())
                                    update = update.Set("Genres", new BsonArray(spotifyArtist.Genres));

                                // Initialize new lists if they don't exist
                                if (!doc.Contains("TopTrackIds")) update = update.Set("TopTrackIds", new BsonArray());

                                if (!doc.Contains("RelatedArtistIds"))
                                    update = update.Set("RelatedArtistIds", new BsonArray());

                                if (!doc.Contains("AlbumIds")) update = update.Set("AlbumIds", new BsonArray());

                                // Apply update
                                await artistsCollection.UpdateOneAsync(
                                    Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                                    update,
                                    cancellationToken: cancellationToken);

                                processed++;
                                if (processed % 50 == 0)
                                    _logger.LogInformation("Processed {Count}/{Total} artists", processed, count);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing raw data for artist {Id}", doc["_id"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing artist document {Id}", doc["_id"]);
                }

        _logger.LogInformation("Completed artist migration: {Count}/{Total} processed", processed, count);
    }

    private string GetOptimalImage(List<SpotifyImage> images)
    {
        if (images == null || !images.Any())
            return null;

        // Try to find a 640x640 image
        var optimalImage = images.FirstOrDefault(img => img.Width == 640 && img.Height == 640);

        // If no 640x640 image exists, take the largest available
        if (optimalImage == null)
            optimalImage = images.OrderByDescending(img => img.Width * img.Height).First();

        return optimalImage.Url;
    }

    public async Task ValidateMigrationAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating migration results...");

        var albumsWithRawData = await _database.GetCollection<BsonDocument>(_settings.AlbumsCollectionName)
            .CountDocumentsAsync(Builders<BsonDocument>.Filter.Exists("RawData", true),
                cancellationToken: cancellationToken);

        var tracksWithRawData = await _database.GetCollection<BsonDocument>(_settings.TracksCollectionName)
            .CountDocumentsAsync(Builders<BsonDocument>.Filter.Exists("RawData", true),
                cancellationToken: cancellationToken);

        var artistsWithRawData = await _database.GetCollection<BsonDocument>(_settings.ArtistsCollectionName)
            .CountDocumentsAsync(Builders<BsonDocument>.Filter.Exists("RawData", true),
                cancellationToken: cancellationToken);

        if (albumsWithRawData > 0 || tracksWithRawData > 0 || artistsWithRawData > 0)
            _logger.LogWarning(
                "Migration incomplete: {AlbumCount} albums, {TrackCount} tracks, and {ArtistCount} artists still have RawData field",
                albumsWithRawData, tracksWithRawData, artistsWithRawData);
        else
            _logger.LogInformation("Migration successfully completed! All RawData fields have been removed.");

        // Check for missing required fields
        var albumsMissingFields = await _database.GetCollection<Album>(_settings.AlbumsCollectionName)
            .CountDocumentsAsync(Builders<Album>.Filter.Or(
                Builders<Album>.Filter.Eq(a => a.SpotifyId, null),
                Builders<Album>.Filter.Eq(a => a.Name, null),
                Builders<Album>.Filter.Eq(a => a.ArtistName, null)
            ), cancellationToken: cancellationToken);

        var tracksMissingFields = await _database.GetCollection<Track>(_settings.TracksCollectionName)
            .CountDocumentsAsync(Builders<Track>.Filter.Or(
                Builders<Track>.Filter.Eq(t => t.SpotifyId, null),
                Builders<Track>.Filter.Eq(t => t.Name, null),
                Builders<Track>.Filter.Eq(t => t.ArtistName, null)
            ), cancellationToken: cancellationToken);

        var artistsMissingFields = await _database.GetCollection<Artist>(_settings.ArtistsCollectionName)
            .CountDocumentsAsync(Builders<Artist>.Filter.Or(
                Builders<Artist>.Filter.Eq(a => a.SpotifyId, null),
                Builders<Artist>.Filter.Eq(a => a.Name, null)
            ), cancellationToken: cancellationToken);

        if (albumsMissingFields > 0 || tracksMissingFields > 0 || artistsMissingFields > 0)
            _logger.LogWarning(
                "Data quality issues found: {AlbumCount} albums, {TrackCount} tracks, and {ArtistCount} artists missing required fields",
                albumsMissingFields, tracksMissingFields, artistsMissingFields);
        else
            _logger.LogInformation("Data quality validation passed! All documents have required fields.");

        // Optionally, check for document size reduction
        await LogCollectionSizes();
    }

    public async Task LogCollectionSizes()
    {
        // This requires admin privileges on the database, so might not work in all environments
        try
        {
            var albumsStats =
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument("collStats",
                    _settings.AlbumsCollectionName));
            var tracksStats =
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument("collStats",
                    _settings.TracksCollectionName));
            var artistsStats =
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument("collStats",
                    _settings.ArtistsCollectionName));

            _logger.LogInformation("Collection sizes after migration:");
            _logger.LogInformation(" - Albums: {Size} MB", albumsStats["size"].ToDouble() / (1024 * 1024));
            _logger.LogInformation(" - Tracks: {Size} MB", tracksStats["size"].ToDouble() / (1024 * 1024));
            _logger.LogInformation(" - Artists: {Size} MB", artistsStats["size"].ToDouble() / (1024 * 1024));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Unable to get collection statistics. This might require additional database permissions.");
        }
    }
}