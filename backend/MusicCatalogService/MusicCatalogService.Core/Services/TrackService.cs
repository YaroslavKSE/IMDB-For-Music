using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

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

    // Existing method - Get track from Spotify or cache
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
        var track = await _catalogRepository.GetTrackBySpotifyIdAsync(spotifyId);
        if (track != null && DateTime.UtcNow < track.CacheExpiresAt)
        {
            _logger.LogInformation("Track {SpotifyId} retrieved from database", spotifyId);

            // Deserialize the raw data to Spotify response
            var trackResponse = JsonSerializer.Deserialize<SpotifyTrackResponse>(
                track.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            var trackDto = MapToTrackDetailDto(trackResponse, track.Id);

            // Store in cache
            await _cacheService.SetAsync(
                cacheKey,
                trackDto,
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

            return trackDto;
        }

        // Fetch from Spotify API
        _logger.LogInformation("Fetching track {SpotifyId} from Spotify API", spotifyId);
        var spotifyTrack = await _spotifyApiClient.GetTrackAsync(spotifyId);
        if (spotifyTrack == null)
        {
            _logger.LogWarning("Track {SpotifyId} not found in Spotify", spotifyId);
            return null;
        }

        // Create or update track entity
        var trackEntity = new Track
        {
            Id = track?.Id ?? Guid.NewGuid(),
            SpotifyId = spotifyTrack.Id,
            Name = spotifyTrack.Name,
            ArtistName = spotifyTrack.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",

            // Get optimal image (640x640 or closest available)
            ThumbnailUrl = GetOptimalImage(spotifyTrack.Album.Images),

            Popularity = spotifyTrack.Popularity,
            LastAccessed = DateTime.UtcNow,
            CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),

            // Track-specific fields
            DurationMs = spotifyTrack.DurationMs,
            IsExplicit = spotifyTrack.Explicit,
            Isrc = spotifyTrack.ExternalIds?.Isrc,

            // Album information
            AlbumId = spotifyTrack.Album.Id,
            AlbumName = spotifyTrack.Album.Name,
            AlbumType = spotifyTrack.Album.AlbumType,
            ReleaseDate = spotifyTrack.Album.ReleaseDate,

            // External URLs
            SpotifyUrl = spotifyTrack.ExternalUrls?.Spotify,

            // Artists
            Artists = spotifyTrack.Artists.Select(a => new SimplifiedArtist
            {
                Id = a.Id,
                Name = a.Name,
                SpotifyUrl = a.ExternalUrls?.Spotify
            }).ToList(),

            // Raw data for future flexibility
            RawData = JsonSerializer.Serialize(spotifyTrack)
        };

        // Save to database as a cached item (not permanent)
        await _catalogRepository.AddOrUpdateTrackAsync(trackEntity);

        // Map to DTO
        var result = MapToTrackDetailDto(spotifyTrack, trackEntity.Id);

        // Cache the result
        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

        return result;
    }

    // Implementation for GetMultipleTracksOverviewAsync - returns simplified track information
    public async Task<MultipleTracksOverviewDto> GetMultipleTracksOverviewAsync(IEnumerable<string> spotifyIds)
    {
        if (spotifyIds == null || !spotifyIds.Any())
        {
            throw new ArgumentException("Track IDs cannot be null or empty", nameof(spotifyIds));
        }

        var result = new MultipleTracksOverviewDto();

        try
        {
            // Deduplicate IDs
            var uniqueIds = spotifyIds.Distinct().ToList();

            // Handle Spotify's 50 track limit per request by chunking if needed
            const int spotifyMaxBatchSize = 50;
            var batches = uniqueIds.Chunk(spotifyMaxBatchSize);

            foreach (var batch in batches)
            {
                // Create batch cache key - use a different key prefix to indicate this is overview data
                var batchCacheKey = $"tracks:overview:{string.Join(",", batch)}";

                // Try to get batch from cache
                var cachedBatch = await _cacheService.GetAsync<List<TrackSummaryDto>>(batchCacheKey);

                if (cachedBatch != null && cachedBatch.Any())
                {
                    _logger.LogInformation("Track overview batch retrieved from cache for {Count} tracks", cachedBatch.Count);
                    result.Tracks.AddRange(cachedBatch);
                    continue;
                }

                // Try to get from database first
                var databaseTracks = await _catalogRepository.GetBatchTracksBySpotifyIdsAsync(batch);
                var validDatabaseTracks = databaseTracks
                    .Where(t => t != null && DateTime.UtcNow < t.CacheExpiresAt)
                    .ToList();

                if (validDatabaseTracks.Count == batch.Length)
                {
                    _logger.LogInformation("Retrieved all {Count} track overviews from database", batch.Length);

                    // Map database tracks to summary DTOs (simplified view)
                    var trackSummaries = validDatabaseTracks.Select(track => new TrackSummaryDto
                    {
                        CatalogItemId = track.Id,
                        SpotifyId = track.SpotifyId,
                        Name = track.Name,
                        ArtistName = track.ArtistName,
                        ImageUrl = track.ThumbnailUrl,
                        DurationMs = track.DurationMs,
                        IsExplicit = track.IsExplicit,
                        AlbumId = track.AlbumId,
                        Popularity = track.Popularity,
                        ExternalUrls = track.SpotifyUrl != null ? new List<string> { track.SpotifyUrl } : null
                    }).ToList();

                    // Add to result
                    result.Tracks.AddRange(trackSummaries);

                    // Cache this batch
                    await _cacheService.SetAsync(
                        batchCacheKey,
                        trackSummaries,
                        TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

                    continue;
                }

                // Fetch missing tracks from Spotify API
                _logger.LogInformation("Fetching batch of {Count} track overviews from Spotify API", batch.Length);

                var spotifyResponse = await _spotifyApiClient.GetMultipleTracksAsync(batch);

                if (spotifyResponse?.Tracks == null || !spotifyResponse.Tracks.Any())
                {
                    _logger.LogWarning("No tracks returned from Spotify for batch of {Count} ids", batch.Length);
                    continue;
                }

                var batchSummaries = new List<TrackSummaryDto>();

                // Process each track from Spotify - map to simplified summary
                foreach (var spotifyTrack in spotifyResponse.Tracks)
                {
                    if (spotifyTrack == null) continue;

                    // Get existing track from database or create new entity
                    var existingTrack = validDatabaseTracks.FirstOrDefault(t => t.SpotifyId == spotifyTrack.Id);
                    var trackEntity = new Track
                    {
                        Id = existingTrack?.Id ?? Guid.NewGuid(),
                        SpotifyId = spotifyTrack.Id,
                        Name = spotifyTrack.Name,
                        ArtistName = spotifyTrack.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                        ThumbnailUrl = GetOptimalImage(spotifyTrack.Album.Images),
                        Popularity = spotifyTrack.Popularity,
                        LastAccessed = DateTime.UtcNow,
                        CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),
                        DurationMs = spotifyTrack.DurationMs,
                        IsExplicit = spotifyTrack.Explicit,
                        Isrc = spotifyTrack.ExternalIds?.Isrc,
                        AlbumId = spotifyTrack.Album.Id,
                        AlbumName = spotifyTrack.Album.Name,
                        AlbumType = spotifyTrack.Album.AlbumType,
                        ReleaseDate = spotifyTrack.Album.ReleaseDate,
                        SpotifyUrl = spotifyTrack.ExternalUrls?.Spotify,
                        Artists = spotifyTrack.Artists.Select(a => new SimplifiedArtist
                        {
                            Id = a.Id,
                            Name = a.Name,
                            SpotifyUrl = a.ExternalUrls?.Spotify
                        }).ToList(),
                        RawData = JsonSerializer.Serialize(spotifyTrack)
                    };

                    // Save to database
                    await _catalogRepository.AddOrUpdateTrackAsync(trackEntity);

                    // Map to a simplified summary DTO
                    var trackSummary = new TrackSummaryDto
                    {
                        CatalogItemId = trackEntity.Id,
                        SpotifyId = spotifyTrack.Id,
                        Name = spotifyTrack.Name,
                        ArtistName = spotifyTrack.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                        ImageUrl = GetOptimalImage(spotifyTrack.Album.Images),
                        Images = spotifyTrack.Album.Images?.Select(img => new ImageDto
                        {
                            Url = img.Url,
                            Height = img.Height,
                            Width = img.Width
                        }).ToList(),
                        DurationMs = spotifyTrack.DurationMs,
                        IsExplicit = spotifyTrack.Explicit,
                        TrackNumber = spotifyTrack.TrackNumber,
                        AlbumId = spotifyTrack.Album.Id,
                        Popularity = spotifyTrack.Popularity,
                        ExternalUrls = spotifyTrack.ExternalUrls?.Spotify != null
                            ? new List<string> { spotifyTrack.ExternalUrls.Spotify }
                            : null
                    };

                    batchSummaries.Add(trackSummary);
                }

                // Add batch results to overall results
                result.Tracks.AddRange(batchSummaries);

                // Cache this batch
                await _cacheService.SetAsync(
                    batchCacheKey,
                    batchSummaries,
                    TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple track overviews");
            throw;
        }
    }

    // Get by catalog ID
    public async Task<TrackDetailDto> GetTrackByCatalogIdAsync(Guid catalogId)
    {
        try
        {
            _logger.LogInformation("Retrieving track with catalog ID: {CatalogId}", catalogId);
            
            // Get track by catalog ID directly from database
            var track = await _catalogRepository.GetTrackByIdAsync(catalogId);
            
            if (track == null)
            {
                _logger.LogWarning("Track with catalog ID {CatalogId} not found", catalogId);
                return null;
            }
            
            // Check if the cached item has expired but is still in the database
            if (DateTime.UtcNow > track.CacheExpiresAt)
            {
                _logger.LogInformation("Track with catalog ID {CatalogId} is expired, refreshing from Spotify", catalogId);
                
                // Try to refresh from Spotify
                var refreshedTrack = await GetTrackAsync(track.SpotifyId);
                if (refreshedTrack != null)
                {
                    return refreshedTrack;
                }
            }
            
            // Deserialize the raw data to Spotify response
            var trackResponse = JsonSerializer.Deserialize<SpotifyTrackResponse>(
                track.RawData,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            
            return MapToTrackDetailDto(trackResponse, track.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving track with catalog ID {CatalogId}", catalogId);
            throw;
        }
    }

    // Save track permanently
    public async Task<TrackDetailDto> SaveTrackAsync(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Permanently saving track with Spotify ID: {SpotifyId}", spotifyId);
            
            // First, ensure we have the track (either from cache, database or Spotify)
            var trackDto = await GetTrackAsync(spotifyId);
            if (trackDto == null)
            {
                _logger.LogWarning("Cannot save track with Spotify ID {SpotifyId}: not found", spotifyId);
                return null;
            }
            
            // Retrieve the entity from the database
            var track = await _catalogRepository.GetTrackBySpotifyIdAsync(spotifyId);
            if (track == null)
            {
                _logger.LogError("Unexpected error: track entity not found after GetTrackAsync succeeded");
                throw new InvalidOperationException($"Track entity with Spotify ID {spotifyId} not found");
            }
            
            // Permanently save to database with extended expiration
            await _catalogRepository.SaveTrackAsync(track);
            
            // Return the track DTO for the API response
            return trackDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving track with Spotify ID {SpotifyId}", spotifyId);
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
    private TrackDetailDto MapToTrackDetailDto(SpotifyTrackResponse track, Guid catalogItemId)
    {
        var artistSummaries = track.Artists.Select(artist => new ArtistSummaryDto
        {
            SpotifyId = artist.Id,
            Name = artist.Name,
            ExternalUrls = artist.ExternalUrls?.Spotify != null
                ? new List<string> {artist.ExternalUrls.Spotify}
                : null
        }).ToList();

        // Get primary artist
        var primaryArtist = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist";

        // Get thumbnail URL (optimized for 640x640)
        var thumbnailUrl = GetOptimalImage(track.Album.Images);

        // Get all images
        var images = track.Album.Images.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList();

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
            ExternalUrls = track.Album.ExternalUrls?.Spotify != null
                ? new List<string> {track.Album.ExternalUrls.Spotify}
                : null
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
            ExternalUrls = track.ExternalUrls?.Spotify != null
                ? new List<string> {track.ExternalUrls.Spotify}
                : null
        };
    }
}