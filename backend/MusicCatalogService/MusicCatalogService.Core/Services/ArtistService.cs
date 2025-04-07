using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Services;

public class ArtistService : IArtistService
{
    private readonly ISpotifyApiClient _spotifyApiClient;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ArtistService> _logger;
    private readonly SpotifySettings _spotifySettings;

    public ArtistService(
        ISpotifyApiClient spotifyApiClient,
        ICatalogRepository catalogRepository,
        ICacheService cacheService,
        ILogger<ArtistService> logger,
        IOptions<SpotifySettings> spotifySettings)
    {
        _spotifyApiClient = spotifyApiClient;
        _catalogRepository = catalogRepository;
        _cacheService = cacheService;
        _logger = logger;
        _spotifySettings = spotifySettings.Value;
    }

    // Get artist from Spotify or cache
    public async Task<ArtistDetailDto> GetArtistAsync(string spotifyId)
    {
        // Generate cache key for this artist
        var cacheKey = $"artist:{spotifyId}";

        // Try to get from cache first
        var cachedArtist = await _cacheService.GetAsync<ArtistDetailDto>(cacheKey);
        if (cachedArtist != null)
        {
            _logger.LogInformation("Artist {SpotifyId} retrieved from cache", spotifyId);
            return cachedArtist;
        }

        // Try to get from database
        var artist = await _catalogRepository.GetArtistBySpotifyIdAsync(spotifyId);
        if (artist != null && DateTime.UtcNow < artist.CacheExpiresAt)
        {
            _logger.LogInformation("Artist {SpotifyId} retrieved from database", spotifyId);

            // Deserialize the raw data to Spotify response
            var artistResponse = JsonSerializer.Deserialize<SpotifyArtistResponse>(
                artist.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            var artistDto = MapToArtistDetailDto(artistResponse, artist.Id);

            // Store in cache
            await _cacheService.SetAsync(
                cacheKey,
                artistDto,
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

            return artistDto;
        }

        // Fetch from Spotify API
        _logger.LogInformation("Fetching artist {SpotifyId} from Spotify API", spotifyId);
        var spotifyArtist = await _spotifyApiClient.GetArtistAsync(spotifyId);
        if (spotifyArtist == null)
        {
            _logger.LogWarning("Artist {SpotifyId} not found in Spotify", spotifyId);
            return null;
        }

        // Create or update artist entity
        var artistEntity = new Artist
        {
            Id = artist?.Id ?? Guid.NewGuid(),
            SpotifyId = spotifyArtist.Id,
            Name = spotifyArtist.Name,
            ArtistName = spotifyArtist.Name,

            // Get optimal image (640x640 or closest available)
            ThumbnailUrl = GetOptimalImage(spotifyArtist.Images),

            Popularity = spotifyArtist.Popularity,
            LastAccessed = DateTime.UtcNow,
            CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),

            // Artist-specific fields
            Genres = spotifyArtist.Genres ?? new List<string>(),
            FollowersCount = spotifyArtist.FollowersCount,
            
            // External URLs
            SpotifyUrl = spotifyArtist.ExternalUrls?.Spotify,

            // Raw data for future flexibility
            RawData = JsonSerializer.Serialize(spotifyArtist)
        };

        // Save to database as a cached item (not permanent)
        await _catalogRepository.AddOrUpdateArtistAsync(artistEntity);

        // Map to DTO
        var result = MapToArtistDetailDto(spotifyArtist, artistEntity.Id);

        // Cache the result
        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

        return result;
    }

    // Get by catalog ID
    public async Task<ArtistDetailDto> GetArtistByCatalogIdAsync(Guid catalogId)
    {
        try
        {
            _logger.LogInformation("Retrieving artist with catalog ID: {CatalogId}", catalogId);
            
            // Get artist by catalog ID directly from database
            var artist = await _catalogRepository.GetArtistByIdAsync(catalogId);
            
            if (artist == null)
            {
                _logger.LogWarning("Artist with catalog ID {CatalogId} not found", catalogId);
                return null;
            }
            
            // Check if the cached item has expired but is still in the database
            if (DateTime.UtcNow > artist.CacheExpiresAt)
            {
                _logger.LogInformation("Artist with catalog ID {CatalogId} is expired, refreshing from Spotify", catalogId);
                
                // Try to refresh from Spotify
                var refreshedArtist = await GetArtistAsync(artist.SpotifyId);
                if (refreshedArtist != null)
                {
                    return refreshedArtist;
                }
            }
            
            // Deserialize the raw data to Spotify response
            var artistResponse = JsonSerializer.Deserialize<SpotifyArtistResponse>(
                artist.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            
            return MapToArtistDetailDto(artistResponse, artist.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artist with catalog ID {CatalogId}", catalogId);
            throw;
        }
    }

    // Save artist permanently
    public async Task<ArtistDetailDto> SaveArtistAsync(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Permanently saving artist with Spotify ID: {SpotifyId}", spotifyId);
            
            // First, ensure we have the artist (either from cache, database or Spotify)
            var artistDto = await GetArtistAsync(spotifyId);
            if (artistDto == null)
            {
                _logger.LogWarning("Cannot save artist with Spotify ID {SpotifyId}: not found", spotifyId);
                return null;
            }
            
            // Retrieve the entity from the database
            var artist = await _catalogRepository.GetArtistBySpotifyIdAsync(spotifyId);
            if (artist == null)
            {
                _logger.LogError("Unexpected error: artist entity not found after GetArtistAsync succeeded");
                throw new InvalidOperationException($"Artist entity with Spotify ID {spotifyId} not found");
            }
            
            // Permanently save to database with extended expiration
            await _catalogRepository.SaveArtistAsync(artist);
            
            // Return the artist DTO for the API response
            return artistDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving artist with Spotify ID {SpotifyId}", spotifyId);
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
    private ArtistDetailDto MapToArtistDetailDto(SpotifyArtistResponse artist, Guid catalogItemId)
    {
        // Get all images (we'll still provide all available sizes in the DTO)
        var images = artist.Images?.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList() ?? new List<ImageDto>();

                    return new ArtistDetailDto
        {
            CatalogItemId = catalogItemId,
            SpotifyId = artist.Id,
            Name = artist.Name,
            ImageUrl = GetOptimalImage(artist.Images),
            Images = images,
            Popularity = artist.Popularity,
            Genres = artist.Genres ?? new List<string>(),
            FollowersCount = artist.FollowersCount,
            ExternalUrls = artist.ExternalUrls?.Spotify != null
                ? new List<string> {artist.ExternalUrls.Spotify}
                : null
        };
    }
}