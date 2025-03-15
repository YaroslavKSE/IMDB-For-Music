using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;

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
        var catalogItem = await _catalogRepository.GetBySpotifyIdAsync(spotifyId, "album");
        if (catalogItem != null && DateTime.UtcNow < catalogItem.CacheExpiresAt)
        {
            _logger.LogInformation("Album {SpotifyId} retrieved from database", spotifyId);
            
            // Deserialize the raw data to Spotify response
            var albumResponse = JsonSerializer.Deserialize<SpotifyAlbumResponse>(
                catalogItem.RawData, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            var albumDto = MapToAlbumDetailDto(albumResponse, catalogItem.Id);
            
            // Store in cache
            await _cacheService.SetAsync(
                cacheKey, 
                albumDto, 
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
            
            return albumDto;
        }
        
        // Fetch from Spotify API
        _logger.LogInformation("Fetching album {SpotifyId} from Spotify API", spotifyId);
        var album = await _spotifyApiClient.GetAlbumAsync(spotifyId);
        if (album == null)
        {
            _logger.LogWarning("Album {SpotifyId} not found in Spotify", spotifyId);
            return null;
        }
        
        // Create or update catalog item
        var itemId = catalogItem?.Id ?? Guid.NewGuid();
        var newCatalogItem = new CatalogItem
        {
            Id = itemId,
            SpotifyId = album.Id,
            Type = "album",
            Name = album.Name,
            ArtistName = album.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
            ThumbnailUrl = album.Images.FirstOrDefault()?.Url,
            Popularity = album.Popularity,
            LastAccessed = DateTime.UtcNow,
            CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),
            RawData = JsonSerializer.Serialize(album)
        };
        
        // Save to database
        await _catalogRepository.AddOrUpdateAsync(newCatalogItem);
        
        // Map to DTO
        var result = MapToAlbumDetailDto(album, itemId);
        
        // Cache the result
        await _cacheService.SetAsync(
            cacheKey, 
            result, 
            TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
        
        return result;
    }
    
    private AlbumDetailDto MapToAlbumDetailDto(SpotifyAlbumResponse album, Guid catalogItemId)
    {
        var artistSummaries = album.Artists.Select(artist => new ArtistSummaryDto
        {
            SpotifyId = artist.Id,
            Name = artist.Name,
            ExternalUrls = artist.ExternalUrls != null ? new List<string> { artist.ExternalUrls.Spotify } : null
        }).ToList();
        
        // Get primary artist
        var primaryArtist = album.Artists.FirstOrDefault()?.Name ?? "Unknown Artist";
        
        // Get album images
        var images = album.Images.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList();
        
        // Get thumbnail (first image)
        var thumbnailUrl = images.FirstOrDefault()?.Url;

        // Map tracks if available
        var tracks = new List<TrackSummaryDto>();
        if (album.Tracks?.Items != null)
        {
            tracks = album.Tracks.Items.Select(track => new TrackSummaryDto
            {
                SpotifyId = track.Id,
                Name = track.Name,
                ArtistName = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                DurationMs = track.DurationMs,
                IsExplicit = track.Explicit,
                TrackNumber = track.TrackNumber,
                ExternalUrls = track.ExternalUrls != null ? new List<string> { track.ExternalUrls.Spotify } : null
            }).ToList();
        }
        
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
            Genres = album.Genres?.ToList(),
            ExternalUrls = album.ExternalUrls != null ? new List<string> { album.ExternalUrls.Spotify } : null
        };
    }
}