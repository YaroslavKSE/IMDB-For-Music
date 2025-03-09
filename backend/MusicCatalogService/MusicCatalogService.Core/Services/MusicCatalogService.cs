using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;

namespace MusicCatalogService.Core.Services;

public class MusicCatalogService : IMusicCatalogService
{
    private readonly ISpotifyApiClient _spotifyClient;
    private readonly ICatalogRepository _catalogRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MusicCatalogService> _logger;
    private readonly SpotifySettings _settings;

    public MusicCatalogService(
        ISpotifyApiClient spotifyClient,
        ICatalogRepository catalogRepository,
        IDistributedCache cache,
        ILogger<MusicCatalogService> logger,
        IOptions<SpotifySettings> settings)
    {
        _spotifyClient = spotifyClient;
        _catalogRepository = catalogRepository;
        _cache = cache;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<CatalogItemDto> GetAlbumAsync(string spotifyId)
    {
        // Try to get from cache first
        var cacheKey = $"album:{spotifyId}";
        var cachedItem = await GetFromCacheAsync<SpotifyAlbumResponse>(cacheKey);

        SpotifyAlbumResponse spotifyAlbum;
        if (cachedItem != null)
        {
            _logger.LogInformation("Cache hit for album {SpotifyId}", spotifyId);
            spotifyAlbum = cachedItem;
        }
        else
        {
            _logger.LogInformation("Cache miss for album {SpotifyId}, fetching from Spotify API", spotifyId);
            spotifyAlbum = await _spotifyClient.GetAlbumAsync(spotifyId);
            await SaveToCacheAsync(cacheKey, spotifyAlbum);
        }

        // Check if we already have this album in our catalog
        var catalogItem = await _catalogRepository.GetBySpotifyIdAsync(spotifyId, "album");

        // If not, create a new catalog entry
        if (catalogItem == null)
        {
            _logger.LogInformation("Creating new catalog entry for album {SpotifyId}", spotifyId);
            
            catalogItem = new CatalogItem
            {
                Id = Guid.NewGuid(),
                SpotifyId = spotifyId,
                Type = "album",
                Name = spotifyAlbum.Name,
                ArtistName = string.Join(", ", spotifyAlbum.Artists.Select(a => a.Name)),
                ThumbnailUrl = spotifyAlbum.Images.FirstOrDefault()?.Url,
                Popularity = spotifyAlbum.Popularity,
                LastAccessed = DateTime.UtcNow,
                CacheExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CacheExpirationMinutes)
            };

            await _catalogRepository.AddAsync(catalogItem);
        }
        else
        {
            // Update LastAccessed timestamp
            catalogItem.LastAccessed = DateTime.UtcNow;
            await _catalogRepository.UpdateAsync(catalogItem);
        }

        // Map to DTO with both catalog and Spotify data
        return new CatalogItemDto
        {
            CatalogItemId = catalogItem.Id,
            SpotifyId = spotifyAlbum.Id,
            Type = "album",
            Name = spotifyAlbum.Name,
            ArtistName = string.Join(", ", spotifyAlbum.Artists.Select(a => a.Name)),
            Artists = spotifyAlbum.Artists.Select(a => new ArtistDto 
            { 
                SpotifyId = a.Id, 
                Name = a.Name 
            }).ToList(),
            ReleaseDate = spotifyAlbum.ReleaseDate,
            ImageUrl = spotifyAlbum.Images.FirstOrDefault()?.Url,
            Popularity = spotifyAlbum.Popularity,
            AlbumType = spotifyAlbum.AlbumType,
            Tracks = spotifyAlbum.Tracks.Items.Select(t => new TrackDto
            {
                SpotifyId = t.Id,
                Name = t.Name,
                DurationMs = t.DurationMs,
                ArtistName = string.Join(", ", t.Artists.Select(a => a.Name))
            }).ToList()
        };
    }

    public async Task<CatalogItemDto> GetTrackAsync(string spotifyId)
    {
        // Similar implementation as GetAlbumAsync
        var cacheKey = $"track:{spotifyId}";
        var cachedItem = await GetFromCacheAsync<SpotifyTrackResponse>(cacheKey);

        SpotifyTrackResponse spotifyTrack;
        if (cachedItem != null)
        {
            _logger.LogInformation("Cache hit for track {SpotifyId}", spotifyId);
            spotifyTrack = cachedItem;
        }
        else
        {
            _logger.LogInformation("Cache miss for track {SpotifyId}, fetching from Spotify API", spotifyId);
            spotifyTrack = await _spotifyClient.GetTrackAsync(spotifyId);
            await SaveToCacheAsync(cacheKey, spotifyTrack);
        }

        // Check if we already have this track in our catalog
        var catalogItem = await _catalogRepository.GetBySpotifyIdAsync(spotifyId, "track");

        // If not, create a new catalog entry
        if (catalogItem == null)
        {
            _logger.LogInformation("Creating new catalog entry for track {SpotifyId}", spotifyId);
            
            catalogItem = new CatalogItem
            {
                Id = Guid.NewGuid(),
                SpotifyId = spotifyId,
                Type = "track",
                Name = spotifyTrack.Name,
                ArtistName = string.Join(", ", spotifyTrack.Artists.Select(a => a.Name)),
                ThumbnailUrl = spotifyTrack.Album.Images.FirstOrDefault()?.Url,
                Popularity = spotifyTrack.Popularity,
                LastAccessed = DateTime.UtcNow,
                CacheExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CacheExpirationMinutes)
            };

            await _catalogRepository.AddAsync(catalogItem);
        }
        else
        {
            // Update LastAccessed timestamp
            catalogItem.LastAccessed = DateTime.UtcNow;
            await _catalogRepository.UpdateAsync(catalogItem);
        }

        // Map to DTO with both catalog and Spotify data
        return new CatalogItemDto
        {
            CatalogItemId = catalogItem.Id,
            SpotifyId = spotifyTrack.Id,
            Type = "track",
            Name = spotifyTrack.Name,
            ArtistName = string.Join(", ", spotifyTrack.Artists.Select(a => a.Name)),
            Artists = spotifyTrack.Artists.Select(a => new ArtistDto 
            { 
                SpotifyId = a.Id, 
                Name = a.Name 
            }).ToList(),
            DurationMs = spotifyTrack.DurationMs,
            ImageUrl = spotifyTrack.Album.Images.FirstOrDefault()?.Url,
            AlbumName = spotifyTrack.Album.Name,
            AlbumId = spotifyTrack.Album.Id,
            Popularity = spotifyTrack.Popularity,
            IsExplicit = spotifyTrack.Explicit
        };
    }

    public async Task<CatalogItemDto> GetArtistAsync(string spotifyId)
    {
        // Implementation for getting artist details, similar to album and track
        var cacheKey = $"artist:{spotifyId}";
        var cachedItem = await GetFromCacheAsync<SpotifyArtistResponse>(cacheKey);

        SpotifyArtistResponse spotifyArtist;
        if (cachedItem != null)
        {
            _logger.LogInformation("Cache hit for artist {SpotifyId}", spotifyId);
            spotifyArtist = cachedItem;
        }
        else
        {
            _logger.LogInformation("Cache miss for artist {SpotifyId}, fetching from Spotify API", spotifyId);
            spotifyArtist = await _spotifyClient.GetArtistAsync(spotifyId);
            await SaveToCacheAsync(cacheKey, spotifyArtist);
        }

        // Check if we already have this artist in our catalog
        var catalogItem = await _catalogRepository.GetBySpotifyIdAsync(spotifyId, "artist");

        // If not, create a new catalog entry
        if (catalogItem == null)
        {
            _logger.LogInformation("Creating new catalog entry for artist {SpotifyId}", spotifyId);
            
            catalogItem = new CatalogItem
            {
                Id = Guid.NewGuid(),
                SpotifyId = spotifyId,
                Type = "artist",
                Name = spotifyArtist.Name,
                ThumbnailUrl = spotifyArtist.Images.FirstOrDefault()?.Url,
                Popularity = spotifyArtist.Popularity,
                LastAccessed = DateTime.UtcNow,
                CacheExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CacheExpirationMinutes)
            };

            await _catalogRepository.AddAsync(catalogItem);
        }
        else
        {
            // Update LastAccessed timestamp
            catalogItem.LastAccessed = DateTime.UtcNow;
            await _catalogRepository.UpdateAsync(catalogItem);
        }

        // Map to DTO with both catalog and Spotify data
        return new CatalogItemDto
        {
            CatalogItemId = catalogItem.Id,
            SpotifyId = spotifyArtist.Id,
            Type = "artist",
            Name = spotifyArtist.Name,
            ImageUrl = spotifyArtist.Images.FirstOrDefault()?.Url,
            Popularity = spotifyArtist.Popularity,
            Genres = spotifyArtist.Genres,
            FollowersCount = spotifyArtist.FollowersCount
        };
    }

    public async Task<SearchResultDto> SearchAsync(string query, string type, int limit = 20, int offset = 0)
    {
        // Implementation for search functionality
        _logger.LogInformation("Searching for {Query} with type {Type}", query, type);
        
        // We don't cache search results as they change frequently
        var searchResults = await _spotifyClient.SearchAsync(query, type, limit, offset);

        var result = new SearchResultDto
        {
            Query = query,
            Type = type,
            Limit = limit,
            Offset = offset,
            TotalResults = 0
        };

        // Map album results if requested
        if (type.Contains("album") && searchResults.Albums != null)
        {
            result.Albums = searchResults.Albums.Items.Select(a => new AlbumDto
            {
                SpotifyId = a.Id,
                Name = a.Name,
                ArtistName = string.Join(", ", a.Artists.Select(artist => artist.Name)),
                ImageUrl = a.Images.FirstOrDefault()?.Url,
                ReleaseDate = a.ReleaseDate,
                AlbumType = a.AlbumType
            }).ToList();
            
            result.TotalResults += searchResults.Albums.Total;
        }

        // Map track results if requested
        if (type.Contains("track") && searchResults.Tracks != null)
        {
            result.Tracks = searchResults.Tracks.Items.Select(t => new TrackDto
            {
                SpotifyId = t.Id,
                Name = t.Name,
                ArtistName = string.Join(", ", t.Artists.Select(artist => artist.Name)),
                DurationMs = t.DurationMs
            }).ToList();
            
            result.TotalResults += searchResults.Tracks.Total;
        }

        // Map artist results if requested
        if (type.Contains("artist") && searchResults.Artists != null)
        {
            result.Artists = searchResults.Artists.Items.Select(a => new ArtistDto
            {
                SpotifyId = a.Id,
                Name = a.Name
            }).ToList();
            
            result.TotalResults += searchResults.Artists.Total;
        }

        return result;
    }

    public async Task<NewReleasesDto> GetNewReleasesAsync(int limit = 20, int offset = 0)
    {
        // Implementation for fetching new releases
        _logger.LogInformation("Fetching new releases, limit: {Limit}, offset: {Offset}", limit, offset);
        
        // Cache new releases for a shorter period (e.g., 1 hour)
        var cacheKey = $"new-releases:{limit}:{offset}";
        var cachedReleases = await GetFromCacheAsync<SpotifyNewReleasesResponse>(cacheKey);

        SpotifyNewReleasesResponse newReleases;
        if (cachedReleases != null)
        {
            _logger.LogInformation("Cache hit for new releases");
            newReleases = cachedReleases;
        }
        else
        {
            _logger.LogInformation("Cache miss for new releases, fetching from Spotify API");
            newReleases = await _spotifyClient.GetNewReleasesAsync(limit, offset);
            
            // Cache for a shorter period since new releases change frequently
            await SaveToCacheAsync(cacheKey, newReleases, TimeSpan.FromHours(1));
        }

        return new NewReleasesDto
        {
            Albums = newReleases.Albums.Items.Select(a => new AlbumDto
            {
                SpotifyId = a.Id,
                Name = a.Name,
                ArtistName = string.Join(", ", a.Artists.Select(artist => artist.Name)),
                ImageUrl = a.Images.FirstOrDefault()?.Url,
                ReleaseDate = a.ReleaseDate,
                AlbumType = a.AlbumType
            }).ToList(),
            Total = newReleases.Albums.Total,
            Limit = newReleases.Albums.Limit,
            Offset = newReleases.Albums.Offset
        };
    }

    // Helper methods for caching
    private async Task<T> GetFromCacheAsync<T>(string key) where T : class
    {
        var cachedData = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cachedData))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(cachedData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task SaveToCacheAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.CacheExpirationMinutes)
        };

        var jsonData = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await _cache.SetStringAsync(key, jsonData, options);
    }
}
