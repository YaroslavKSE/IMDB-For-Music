using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Services;

public class AlbumService : IAlbumService
{
    private readonly ISpotifyApiClient _spotifyApiClient;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AlbumService> _logger;
    private readonly SpotifySettings _spotifySettings;

    public AlbumService(
        ISpotifyApiClient spotifyApiClient,
        ICatalogRepository catalogRepository,
        ICacheService cacheService,
        ILogger<AlbumService> logger,
        IOptions<SpotifySettings> spotifySettings)
    {
        _spotifyApiClient = spotifyApiClient;
        _catalogRepository = catalogRepository;
        _cacheService = cacheService;
        _logger = logger;
        _spotifySettings = spotifySettings.Value;
    }

    // Get album from Spotify or cache
    public async Task<AlbumDetailDto> GetAlbumAsync(string spotifyId)
    {
        // Generate cache key for this album
        var cacheKey = $"album:{spotifyId}";

        // Try to get from cache first
        var cachedAlbum = await _cacheService.GetAsync<AlbumDetailDto>(cacheKey);
        if (cachedAlbum != null)
        {
            _logger.LogInformation("Album {SpotifyId} retrieved from cache", spotifyId);
            return cachedAlbum;
        }

        // Try to get from database
        var album = await _catalogRepository.GetAlbumBySpotifyIdAsync(spotifyId);
        if (album != null && DateTime.UtcNow < album.CacheExpiresAt)
        {
            _logger.LogInformation("Album {SpotifyId} retrieved from database", spotifyId);

            // Deserialize the raw data to Spotify response
            var albumResponse = JsonSerializer.Deserialize<SpotifyAlbumResponse>(
                album.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            var albumDto = MapToAlbumDetailDto(albumResponse, album.Id);

            // Store in cache
            await _cacheService.SetAsync(
                cacheKey,
                albumDto,
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

            return albumDto;
        }

        // Fetch from Spotify API
        _logger.LogInformation("Fetching album {SpotifyId} from Spotify API", spotifyId);
        var spotifyAlbum = await _spotifyApiClient.GetAlbumAsync(spotifyId);
        if (spotifyAlbum == null)
        {
            _logger.LogWarning("Album {SpotifyId} not found in Spotify", spotifyId);
            return null;
        }

        // Create or update album entity
        var albumEntity = new Album
        {
            Id = album?.Id ?? Guid.NewGuid(),
            SpotifyId = spotifyAlbum.Id,
            Name = spotifyAlbum.Name,
            ArtistName = spotifyAlbum.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",

            // Get optimal image (640x640 or closest available)
            ThumbnailUrl = GetOptimalImage(spotifyAlbum.Images),

            Popularity = spotifyAlbum.Popularity,
            LastAccessed = DateTime.UtcNow,
            CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),

            // Album-specific fields
            ReleaseDate = spotifyAlbum.ReleaseDate,
            ReleaseDatePrecision = spotifyAlbum.ReleaseDatePrecision,
            AlbumType = spotifyAlbum.AlbumType,
            TotalTracks = spotifyAlbum.TotalTracks,
            Label = spotifyAlbum.Label,
            Copyright = spotifyAlbum.Copyright,

            // External URLs
            SpotifyUrl = spotifyAlbum.ExternalUrls?.Spotify,

            // Artists
            Artists = spotifyAlbum.Artists.Select(a => new SimplifiedArtist
            {
                Id = a.Id,
                Name = a.Name,
                SpotifyUrl = a.ExternalUrls?.Spotify
            }).ToList(),

            // Genres - Deprecated from Spotify
            // Genres = spotifyAlbum.Genres?.ToList() ?? new List<string>(),

            // Raw data for future flexibility
            RawData = JsonSerializer.Serialize(spotifyAlbum)
        };

        // Save to database as a cached item (not permanent)
        await _catalogRepository.AddOrUpdateAlbumAsync(albumEntity);

        // Map to DTO
        var result = MapToAlbumDetailDto(spotifyAlbum, albumEntity.Id);

        // Cache the result
        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

        return result;
    }

    // Get by catalog ID
    public async Task<AlbumDetailDto> GetAlbumByCatalogIdAsync(Guid catalogId)
    {
        try
        {
            _logger.LogInformation("Retrieving album with catalog ID: {CatalogId}", catalogId);
            
            // Get album by catalog ID directly from database
            var album = await _catalogRepository.GetAlbumByIdAsync(catalogId);
            
            if (album == null)
            {
                _logger.LogWarning("Album with catalog ID {CatalogId} not found", catalogId);
                return null;
            }
            
            // Check if the cached item has expired but is still in the database
            if (DateTime.UtcNow > album.CacheExpiresAt)
            {
                _logger.LogInformation("Album with catalog ID {CatalogId} is expired, refreshing from Spotify", catalogId);
                
                // Try to refresh from Spotify
                var refreshedAlbum = await GetAlbumAsync(album.SpotifyId);
                if (refreshedAlbum != null)
                {
                    return refreshedAlbum;
                }
            }
            
            // Deserialize the raw data to Spotify response
            var albumResponse = JsonSerializer.Deserialize<SpotifyAlbumResponse>(
                album.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            
            return MapToAlbumDetailDto(albumResponse, album.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving album with catalog ID {CatalogId}", catalogId);
            throw;
        }
    }

    // Save album permanently
    public async Task<AlbumDetailDto> SaveAlbumAsync(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Permanently saving album with Spotify ID: {SpotifyId}", spotifyId);
            
            // First, ensure we have the album (either from cache, database or Spotify)
            var albumDto = await GetAlbumAsync(spotifyId);
            if (albumDto == null)
            {
                _logger.LogWarning("Cannot save album with Spotify ID {SpotifyId}: not found", spotifyId);
                return null;
            }
            
            // Retrieve the entity from the database
            var album = await _catalogRepository.GetAlbumBySpotifyIdAsync(spotifyId);
            if (album == null)
            {
                _logger.LogError("Unexpected error: album entity not found after GetAlbumAsync succeeded");
                throw new InvalidOperationException($"Album entity with Spotify ID {spotifyId} not found");
            }
            
            // Permanently save to database with extended expiration
            await _catalogRepository.SaveAlbumAsync(album);
            
            // Return the album DTO for the API response
            return albumDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving album with Spotify ID {SpotifyId}", spotifyId);
            throw;
        }
    }

    // Helper method to get the optimal image (640x640 or closest)
    private string GetOptimalImage(List<SpotifyImage> images)
    {
        if (images == null || !images.Any())
            return null;

        // Try to find a 640x640 image
        var optimalImage = images.FirstOrDefault(img => img.Width == 640 && img.Height == 640);

        // If no 640x640 image exists, take the largest available
        if (optimalImage == null) optimalImage = images.OrderByDescending(img => img.Width * img.Height).First();

        return optimalImage.Url;
    }

    // Map Spotify response to DTO
    private AlbumDetailDto MapToAlbumDetailDto(SpotifyAlbumResponse album, Guid catalogItemId)
    {
        // Get all artists
        var artistSummaries = album.Artists.Select(artist => new ArtistSummaryDto
        {
            SpotifyId = artist.Id,
            Name = artist.Name,
            ExternalUrls = artist.ExternalUrls?.Spotify != null
                ? new List<string> {artist.ExternalUrls.Spotify}
                : null
        }).ToList();

        // Get primary artist
        var primaryArtist = album.Artists.FirstOrDefault()?.Name ?? "Unknown Artist";

        // Get thumbnail URL (optimized for 640x640)
        var thumbnailUrl = GetOptimalImage(album.Images);

        // Get all images (we'll still provide all available sizes in the DTO)
        var images = album.Images.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList();

        // Map tracks if available
        var tracks = new List<TrackSummaryDto>();
        if (album.Tracks?.Items != null)
            tracks = album.Tracks.Items.Select(track => new TrackSummaryDto
            {
                SpotifyId = track.Id,
                Name = track.Name,
                ArtistName = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                DurationMs = track.DurationMs,
                IsExplicit = track.Explicit,
                TrackNumber = track.TrackNumber,
                ExternalUrls = track.ExternalUrls?.Spotify != null
                    ? new List<string> {track.ExternalUrls.Spotify}
                    : null
            }).ToList();

        return new AlbumDetailDto
        {
            CatalogItemId = catalogItemId,
            SpotifyId = album.Id,
            Name = album.Name,
            ArtistName = primaryArtist,
            ImageUrl = thumbnailUrl,
            Images = images,
            Popularity = album.Popularity,
            ReleaseDate = album.ReleaseDate,
            ReleaseDatePrecision = album.ReleaseDatePrecision,
            AlbumType = album.AlbumType,
            TotalTracks = album.TotalTracks,
            Label = album.Label,
            Copyright = album.Copyright,
            Artists = artistSummaries,
            Tracks = tracks,
            // Genres = album.Genres?.ToList(),
            ExternalUrls = album.ExternalUrls?.Spotify != null
                ? new List<string> {album.ExternalUrls.Spotify}
                : null
        };
    }
}