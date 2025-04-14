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

    public async Task<AlbumTracksResultDto> GetAlbumTracksAsync(string spotifyId, int limit = 20, int offset = 0, string? market = null)
    {
        try
        {
            // Generate cache key for this request
            var cacheKey = $"album:{spotifyId}:tracks:{limit}:{offset}:{market ?? "none"}";

            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<AlbumTracksResultDto>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Album tracks for {SpotifyId} retrieved from cache", spotifyId);
                return cachedResult;
            }

            // Get the album to ensure it exists and to get the name
            var album = await _catalogRepository.GetAlbumBySpotifyIdAsync(spotifyId);
            string albumName = "Unknown Album";

            if (album != null)
            {
                albumName = album.Name;
            }
            else
            {
                // Try to fetch album from Spotify
                var albumResponse = await _spotifyApiClient.GetAlbumAsync(spotifyId);
                if (albumResponse != null)
                {
                    albumName = albumResponse.Name;
                }
            }

            // Fetch tracks from Spotify API
            _logger.LogInformation("Fetching tracks for album {SpotifyId} from Spotify API", spotifyId);
            var tracksResponse = await _spotifyApiClient.GetAlbumTracksAsync(spotifyId, limit, offset, market);
            if (tracksResponse == null)
            {
                _logger.LogWarning("No tracks found for album {SpotifyId}", spotifyId);
                return new AlbumTracksResultDto
                {
                    AlbumId = spotifyId,
                    AlbumName = albumName,
                    Limit = limit,
                    Offset = offset,
                    TotalResults = 0
                };
            }

            // Map the response to our DTO
            var result = MapToAlbumTracksResultDto(tracksResponse, spotifyId, albumName, limit, offset);

            // Cache the result
            await _cacheService.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tracks for album {SpotifyId}", spotifyId);
            throw;
        }
    }

    // Implementation for GetMultipleAlbumsOverviewAsync - returns simplified album information
    public async Task<MultipleAlbumsOverviewDto> GetMultipleAlbumsOverviewAsync(IEnumerable<string> spotifyIds)
    {
        if (spotifyIds == null || !spotifyIds.Any())
        {
            throw new ArgumentException("Album IDs cannot be null or empty", nameof(spotifyIds));
        }

        var result = new MultipleAlbumsOverviewDto();

        try
        {
            // Deduplicate IDs
            var uniqueIds = spotifyIds.Distinct().ToList();

            // Handle Spotify's 20 album limit per request by chunking if needed
            const int spotifyMaxBatchSize = 20;
            var batches = uniqueIds.Chunk(spotifyMaxBatchSize);

            foreach (var batch in batches)
            {
                // Create batch cache key - use a different key prefix to indicate this is overview data
                var batchCacheKey = $"albums:overview:{string.Join(",", batch)}";

                // Try to get batch from cache
                var cachedBatch = await _cacheService.GetAsync<List<AlbumSummaryDto>>(batchCacheKey);

                if (cachedBatch != null && cachedBatch.Any())
                {
                    _logger.LogInformation("Album overview batch retrieved from cache for {Count} albums",
                        cachedBatch.Count);
                    result.Albums.AddRange(cachedBatch);
                    continue;
                }

                // Try to get from database first
                var databaseAlbums = await _catalogRepository.GetBatchAlbumsBySpotifyIdsAsync(batch);

                // Keep track of which IDs we need to fetch from Spotify
                var missingIds = new List<string>();
                var existingDbAlbums = new Dictionary<string, Album>();
                var albumSummaries = new List<AlbumSummaryDto>();

                // Process database results first and identify missing albums
                foreach (var spotifyId in batch)
                {
                    var dbAlbum = databaseAlbums.FirstOrDefault(a => a != null && a.SpotifyId == spotifyId);

                    if (dbAlbum != null && DateTime.UtcNow < dbAlbum.CacheExpiresAt)
                    {
                        // Valid album from database - add to result directly
                        existingDbAlbums[spotifyId] = dbAlbum;

                        // Map to summary DTO
                        albumSummaries.Add(new AlbumSummaryDto
                        {
                            CatalogItemId = dbAlbum.Id,
                            SpotifyId = dbAlbum.SpotifyId,
                            Name = dbAlbum.Name,
                            ArtistName = dbAlbum.ArtistName,
                            ImageUrl = dbAlbum.ThumbnailUrl,
                            ReleaseDate = dbAlbum.ReleaseDate,
                            AlbumType = dbAlbum.AlbumType,
                            TotalTracks = dbAlbum.TotalTracks,
                            Popularity = dbAlbum.Popularity,
                            ExternalUrls = dbAlbum.SpotifyUrl != null ? new List<string> {dbAlbum.SpotifyUrl} : null
                        });
                    }
                    else
                    {
                        // Album not in database or expired - need to fetch from Spotify
                        missingIds.Add(spotifyId);

                        // Store reference to expired album if it exists (for _id preservation)
                        if (dbAlbum != null) existingDbAlbums[spotifyId] = dbAlbum;
                    }
                }

                // If we got all albums from the database, no need to call Spotify API
                if (!missingIds.Any())
                {
                    _logger.LogInformation("Retrieved all {Count} album overviews from database", albumSummaries.Count);

                    // Add to result
                    result.Albums.AddRange(albumSummaries);

                    // Cache this batch
                    await _cacheService.SetAsync(
                        batchCacheKey,
                        albumSummaries,
                        TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));

                    continue;
                }

                // Fetch missing albums from Spotify API
                _logger.LogInformation("Fetching {Count} missing albums from Spotify API", missingIds.Count);

                var spotifyResponse = missingIds.Count > 0
                    ? await _spotifyApiClient.GetMultipleAlbumsAsync(missingIds)
                    : null;

                if (spotifyResponse?.Albums != null && spotifyResponse.Albums.Any())
                    // Process each album from Spotify
                    foreach (var spotifyAlbum in spotifyResponse.Albums)
                    {
                        if (spotifyAlbum == null) continue;

                        Album albumEntity;

                        // Check if we have an existing entity to update
                        if (existingDbAlbums.TryGetValue(spotifyAlbum.Id, out var existingAlbum))
                        {
                            // Update existing entity properties
                            albumEntity = existingAlbum;
                            albumEntity.Name = spotifyAlbum.Name;
                            albumEntity.ArtistName = spotifyAlbum.Artists.FirstOrDefault()?.Name ?? "Unknown Artist";
                            albumEntity.ThumbnailUrl = GetOptimalImage(spotifyAlbum.Images);
                            albumEntity.Popularity = spotifyAlbum.Popularity;
                            albumEntity.LastAccessed = DateTime.UtcNow;
                            albumEntity.CacheExpiresAt =
                                DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes);
                            albumEntity.ReleaseDate = spotifyAlbum.ReleaseDate;
                            albumEntity.ReleaseDatePrecision = spotifyAlbum.ReleaseDatePrecision;
                            albumEntity.AlbumType = spotifyAlbum.AlbumType;
                            albumEntity.TotalTracks = spotifyAlbum.TotalTracks;
                            albumEntity.Label = spotifyAlbum.Label;
                            albumEntity.Copyright = spotifyAlbum.Copyright;
                            albumEntity.SpotifyUrl = spotifyAlbum.ExternalUrls?.Spotify;
                            albumEntity.Artists = spotifyAlbum.Artists.Select(a => new SimplifiedArtist
                            {
                                Id = a.Id,
                                Name = a.Name,
                                SpotifyUrl = a.ExternalUrls?.Spotify
                            }).ToList();
                            albumEntity.RawData = JsonSerializer.Serialize(spotifyAlbum);
                        }
                        else
                        {
                            // Create new entity for albums not in database
                            albumEntity = new Album
                            {
                                Id = Guid.NewGuid(),
                                SpotifyId = spotifyAlbum.Id,
                                Name = spotifyAlbum.Name,
                                ArtistName = spotifyAlbum.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                                ThumbnailUrl = GetOptimalImage(spotifyAlbum.Images),
                                Popularity = spotifyAlbum.Popularity,
                                LastAccessed = DateTime.UtcNow,
                                CacheExpiresAt = DateTime.UtcNow.AddMinutes(_spotifySettings.CacheExpirationMinutes),
                                ReleaseDate = spotifyAlbum.ReleaseDate,
                                ReleaseDatePrecision = spotifyAlbum.ReleaseDatePrecision,
                                AlbumType = spotifyAlbum.AlbumType,
                                TotalTracks = spotifyAlbum.TotalTracks,
                                Label = spotifyAlbum.Label,
                                Copyright = spotifyAlbum.Copyright,
                                SpotifyUrl = spotifyAlbum.ExternalUrls?.Spotify,
                                Artists = spotifyAlbum.Artists.Select(a => new SimplifiedArtist
                                {
                                    Id = a.Id,
                                    Name = a.Name,
                                    SpotifyUrl = a.ExternalUrls?.Spotify
                                }).ToList(),
                                RawData = JsonSerializer.Serialize(spotifyAlbum)
                            };
                        }

                        // Save to database
                        await _catalogRepository.AddOrUpdateAlbumAsync(albumEntity);

                        // Map to a summary DTO and add to results
                        var albumSummary = new AlbumSummaryDto
                        {
                            CatalogItemId = albumEntity.Id,
                            SpotifyId = spotifyAlbum.Id,
                            Name = spotifyAlbum.Name,
                            ArtistName = spotifyAlbum.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                            ImageUrl = GetOptimalImage(spotifyAlbum.Images),
                            Images = spotifyAlbum.Images?.Select(img => new ImageDto
                            {
                                Url = img.Url,
                                Height = img.Height,
                                Width = img.Width
                            }).ToList(),
                            ReleaseDate = spotifyAlbum.ReleaseDate,
                            AlbumType = spotifyAlbum.AlbumType,
                            TotalTracks = spotifyAlbum.TotalTracks,
                            Popularity = spotifyAlbum.Popularity,
                            ExternalUrls = spotifyAlbum.ExternalUrls?.Spotify != null
                                ? new List<string> {spotifyAlbum.ExternalUrls.Spotify}
                                : null
                        };

                        albumSummaries.Add(albumSummary);
                    }

                // Add all album summaries to the result (both from DB and Spotify)
                result.Albums.AddRange(albumSummaries);

                // Cache this batch
                await _cacheService.SetAsync(
                    batchCacheKey,
                    albumSummaries,
                    TimeSpan.FromMinutes(_spotifySettings.CacheExpirationMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple album overviews");
            throw;
        }
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
                _logger.LogInformation("Album with catalog ID {CatalogId} is expired, refreshing from Spotify",
                    catalogId);

                // Try to refresh from Spotify
                var refreshedAlbum = await GetAlbumAsync(album.SpotifyId);
                if (refreshedAlbum != null) return refreshedAlbum;
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

    private AlbumTracksResultDto MapToAlbumTracksResultDto(
        SpotifyPagingObject<SpotifyTrackSimplified> response,
        string albumId,
        string albumName,
        int limit,
        int offset)
    {
        var result = new AlbumTracksResultDto
        {
            AlbumId = albumId,
            AlbumName = albumName,
            Limit = limit,
            Offset = offset,
            TotalResults = response.Total,
            Next = response.Next,
            Previous = response.Previous
        };

        // Map tracks
        if (response.Items != null)
        {
            result.Tracks = response.Items.Select(track => new TrackSummaryDto
            {
                SpotifyId = track.Id,
                Name = track.Name,
                ArtistName = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                DurationMs = track.DurationMs,
                IsExplicit = track.Explicit,
                TrackNumber = track.TrackNumber,
                AlbumId = albumId,
                ExternalUrls = track.ExternalUrls?.Spotify != null ? new List<string> { track.ExternalUrls.Spotify } : null
            }).ToList();
        }

        return result;
    }
}