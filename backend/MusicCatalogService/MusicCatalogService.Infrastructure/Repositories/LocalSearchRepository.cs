using MongoDB.Driver;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.Mappers;
using MusicCatalogService.Infrastructure.Configuration;

namespace MusicCatalogService.Infrastructure.Repositories;

public class LocalSearchRepository : ILocalSearchRepository
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public LocalSearchRepository(
        IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        _settings = settings.Value;
    }

    public async Task<SearchResultDto> SearchLocalCatalogAsync(
        string query, 
        string type, 
        int limit = 20, 
        int offset = 0)
    {
        var result = new SearchResultDto
        {
            Query = query,
            Type = type,
            Limit = limit,
            Offset = offset
        };

        // Normalize query for case-insensitive, partial matching
        var normalizedQuery = query.ToLower();

        // Perform search based on type
        var types = type.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var searchType in types)
        {
            switch (searchType.ToLower())
            {
                case "album":
                    result.Albums = await SearchAlbumsAsync(normalizedQuery, limit, offset);
                    result.TotalResults += result.Albums.Count;
                    break;
                case "track":
                    result.Tracks = await SearchTracksAsync(normalizedQuery, limit, offset);
                    result.TotalResults += result.Tracks.Count;
                    break;
                case "artist":
                    result.Artists = await SearchArtistsAsync(normalizedQuery, limit, offset);
                    result.TotalResults += result.Artists.Count;
                    break;
            }
        }

        return result;
    }

    private async Task<List<AlbumSummaryDto>> SearchAlbumsAsync(string query, int limit, int offset)
    {
        var albumsCollection = _database.GetCollection<Album>(_settings.AlbumsCollectionName);

        var filter = Builders<Album>.Filter.Or(
            Builders<Album>.Filter.Regex(a => a.Name, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Album>.Filter.Regex(a => a.ArtistName, new MongoDB.Bson.BsonRegularExpression(query, "i"))
        );

        var albums = await albumsCollection
            .Find(filter)
            .Skip(offset)
            .Limit(limit)
            .ToListAsync();

        return albums.Select(AlbumMapper.MapToAlbumSummaryDto).ToList();
    }

    private async Task<List<TrackSummaryDto>> SearchTracksAsync(string query, int limit, int offset)
    {
        var tracksCollection = _database.GetCollection<Track>(_settings.TracksCollectionName);

        var filter = Builders<Track>.Filter.Or(
            Builders<Track>.Filter.Regex(t => t.Name, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Track>.Filter.Regex(t => t.ArtistName, new MongoDB.Bson.BsonRegularExpression(query, "i"))
        );

        var tracks = await tracksCollection
            .Find(filter)
            .Skip(offset)
            .Limit(limit)
            .ToListAsync();

        return tracks.Select(TrackMapper.MapToTrackSummaryDto).ToList();
    }

    private async Task<List<ArtistSummaryDto>> SearchArtistsAsync(string query, int limit, int offset)
    {
        var artistsCollection = _database.GetCollection<Artist>(_settings.ArtistsCollectionName);

        var filter = Builders<Artist>.Filter.Regex(a => a.Name, new MongoDB.Bson.BsonRegularExpression(query, "i"));

        var artists = await artistsCollection
            .Find(filter)
            .Skip(offset)
            .Limit(limit)
            .ToListAsync();

        return artists.Select(ArtistMapper.MapArtistEntityToDto).ToList<ArtistSummaryDto>();
    }
}