// MusicCatalogService.Core/Services/TrackService.cs
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;

namespace MusicCatalogService.Core.Services;

public class TrackService : ITrackService
{
    private readonly ISpotifyApiClient _spotifyApiClient;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TrackService> _logger;
    private readonly SpotifySettings _spotifySettings;

    public TrackService(
        ISpotifyApiClient spotifyApiClient,
        ICatalogRepository catalogRepository,
        ICacheService cacheService,
        ILogger<TrackService> logger,
        IOptions<SpotifySettings> spotifySettings)
    {
        _spotifyApiClient = spotifyApiClient;
        _catalogRepository = catalogRepository;
        _cacheService = cacheService;
        _logger = logger;
        _spotifySettings = spotifySettings.Value;
    }

    public async Task<TrackDetailDto> GetTrackAsync(string spotifyId)
    {
        // Generate cache key for this track
        var cacheKey = $"track:{spotifyId}";
        
        // Try to get from cache first
        var cachedTrack = await _cacheService.GetAsync<TrackDetailDto>(cacheKey);
        if (cachedTrack != null)
        {
            _logger.LogInformation("Track {SpotifyId} retrieved from cache", spotifyId);
            return cachedTrack;
        }
        
        // Try to get from database
        var catalogItem = await _catalogRepository.GetBySpotifyIdAsync(spotifyId, "track");
        if (catalogItem != null && DateTime.UtcNow < catalogItem.CacheExpiresAt)
        {
            _logger.LogInformation("Track {SpotifyId} retrieved from database", spotifyId);
            
            // Deserialize the raw data to Spotify response
            var trackResponse = JsonSerializer.Deserialize<SpotifyTrackResponse>(
                catalogItem.RawData, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            var trackDto = MapToTrackDetailDto(trackResponse, catalogItem.Id);
            
            // Store in cache
            await _cacheService.SetAsync(
                cacheKey, 
                trackDto, 
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
            
            return trackDto;
        }
        
        // Fetch from Spotify API
        _logger.LogInformation("Fetching track {SpotifyId} from Spotify API", spotifyId);
        var track = await _spotifyApiClient.GetTrackAsync(spotifyId);
        if (track == null)
        {
            _logger.LogWarning("Track {SpotifyId} not found in Spotify", spotifyId);
            return null;
        }
        
        // Create or update catalog item
        var itemId = catalogItem?.Id ?? Guid.NewGuid();
        var newCatalogItem = new CatalogItem
        {
            Id = itemId,
            SpotifyId = track.Id,
            Type = "track",
            Name = track.Name,
            ArtistName = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
            ThumbnailUrl = track.Album.Images.FirstOrDefault()?.Url,
            Popularity = track.Popularity,
            LastAccessed = DateTime.UtcNow,
            CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),
            DurationMs = track.DurationMs,
            IsExplicit = track.Explicit,
            TrackNumber = track.TrackNumber,
            DiscNumber = track.DiscNumber,
            Isrc = track.ExternalIds?.Isrc,
            PreviewUrl = track.PreviewUrl,
            AlbumId = track.Album.Id,
            AlbumName = track.Album.Name,
            RawData = JsonSerializer.Serialize(track)
        };
        
        // Save to database
        await _catalogRepository.AddOrUpdateAsync(newCatalogItem);
        
        // Map to DTO
        var result = MapToTrackDetailDto(track, itemId);
        
        // Cache the result
        await _cacheService.SetAsync(
            cacheKey, 
            result, 
            TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
        
        return result;
    }
    
    private TrackDetailDto MapToTrackDetailDto(SpotifyTrackResponse track, Guid catalogItemId)
    {
        var artistSummaries = track.Artists.Select(artist => new ArtistSummaryDto
        {
            SpotifyId = artist.Id,
            Name = artist.Name,
            ExternalUrls = artist.ExternalUrls != null ? new List<string> { artist.ExternalUrls.Spotify } : null
        }).ToList();
        
        // Get primary artist
        var primaryArtist = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist";
        
        // Get album images
        var images = track.Album.Images.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList();
        
        // Get thumbnail (first image)
        var thumbnailUrl = images.FirstOrDefault()?.Url;
        
        var albumDto = new AlbumSummaryDto
        {
            SpotifyId = track.Album.Id,
            Name = track.Album.Name,
            ArtistName = primaryArtist,
            ReleaseDate = track.Album.ReleaseDate,
            AlbumType = track.Album.AlbumType,
            TotalTracks = track.Album.TotalTracks,
            ImageUrl = thumbnailUrl,
            Images = images,
            ExternalUrls = track.Album.ExternalUrls != null ? new List<string> { track.Album.ExternalUrls.Spotify } : null
        };
        
        return new TrackDetailDto
        {
            CatalogItemId = catalogItemId,
            SpotifyId = track.Id,
            Name = track.Name,
            ArtistName = primaryArtist,
            ImageUrl = thumbnailUrl,
            Images = images,
            Popularity = track.Popularity,
            DurationMs = track.DurationMs,
            IsExplicit = track.Explicit,
            TrackNumber = track.TrackNumber,
            DiscNumber = track.DiscNumber,
            Isrc = track.ExternalIds?.Isrc,
            PreviewUrl = track.PreviewUrl,
            Artists = artistSummaries,
            Album = albumDto,
            ExternalUrls = track.ExternalUrls != null ? new List<string> { track.ExternalUrls.Spotify } : null
        };
    }
}