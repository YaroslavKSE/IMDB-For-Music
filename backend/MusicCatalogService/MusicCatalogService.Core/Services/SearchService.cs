using Microsoft.Extensions.Logging;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;

namespace MusicCatalogService.Core.Services;

public class SearchService : ISearchService
{
    private readonly ISpotifyApiClient _spotifyApiClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        ISpotifyApiClient spotifyApiClient,
        ICacheService cacheService,
        ILogger<SearchService> logger)
    {
        _spotifyApiClient = spotifyApiClient;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SearchResultDto> SearchAsync(string query, string type, int limit = 20, int offset = 0,
        string market = null)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Search query cannot be empty", nameof(query));

        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Search type cannot be empty", nameof(type));

        // Ensure limit is within acceptable range
        limit = Math.Clamp(limit, 1, 50);

        // Generate cache key based on search parameters
        var cacheKey = $"search:{query}:{type}:{limit}:{offset}:{market ?? "none"}";

        // Try to get from cache first
        var cachedResult = await _cacheService.GetAsync<SearchResultDto>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Search results for query '{Query}' retrieved from cache", query);
            return cachedResult;
        }

        // Perform search via Spotify API
        _logger.LogInformation("Performing search for query '{Query}' with type '{Type}'", query, type);
        var searchResponse = await _spotifyApiClient.SearchAsync(query, type, limit, offset);

        if (searchResponse == null)
        {
            _logger.LogWarning("No search results found for query '{Query}'", query);
            return new SearchResultDto
            {
                Query = query,
                Type = type,
                Limit = limit,
                Offset = offset,
                TotalResults = 0
            };
        }

        // Map the search response to our DTO
        var result = MapToSearchResultDto(searchResponse, query, type, limit, offset);

        // Cache the result for a short period (searches are more dynamic than entity lookups)
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    private SearchResultDto MapToSearchResultDto(SpotifySearchResponse response, string query, string type, int limit,
        int offset)
    {
        var result = new SearchResultDto
        {
            Query = query,
            Type = type,
            Limit = limit,
            Offset = offset,
            TotalResults = 0
        };

        // Helper method to get optimal image
        string GetOptimalImage(List<SpotifyImage> images)
        {
            if (images == null || !images.Any())
                return null;

            // Try to find a 640x640 image
            var optimalImage = images.FirstOrDefault(img => img.Width == 640 && img.Height == 640);

            // If no 640x640 image exists, take the largest available
            if (optimalImage == null) optimalImage = images.OrderByDescending(img => img.Width * img.Height).First();

            return optimalImage.Url;
        }

        // Map albums if present
        if (response.Albums != null)
        {
            result.Albums = response.Albums.Items.Select(album => new AlbumSummaryDto
            {
                SpotifyId = album.Id,
                Name = album.Name,
                ArtistName = album.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                ReleaseDate = album.ReleaseDate,
                AlbumType = album.AlbumType,
                TotalTracks = album.TotalTracks,
                ImageUrl = GetOptimalImage(album.Images),
                Images = album.Images.Select(img => new ImageDto
                {
                    Url = img.Url,
                    Height = img.Height,
                    Width = img.Width
                }).ToList(),
                ExternalUrls = album.ExternalUrls != null ? new List<string> {album.ExternalUrls.Spotify} : null
            }).ToList();

            result.TotalResults += response.Albums.Total;
        }

        // Map artists if present
        if (response.Artists != null)
        {
            result.Artists = response.Artists.Items.Select(artist => new ArtistSummaryDto
            {
                SpotifyId = artist.Id,
                Name = artist.Name,
                Popularity = artist.Popularity,
                ImageUrl = GetOptimalImage(artist.Images),
                Images = artist.Images?.Select(img => new ImageDto
                {
                    Url = img.Url,
                    Height = img.Height,
                    Width = img.Width
                }).ToList() ?? new List<ImageDto>(),
                ExternalUrls = artist.ExternalUrls != null ? new List<string> {artist.ExternalUrls.Spotify} : null
            }).ToList();

            result.TotalResults += response.Artists.Total;
        }

        // Map tracks if present
        if (response.Tracks != null)
        {
            result.Tracks = response.Tracks.Items.Select(track => new TrackSummaryDto
            {
                SpotifyId = track.Id,
                Name = track.Name,
                ArtistName = track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                DurationMs = track.DurationMs,
                IsExplicit = track.Explicit,
                TrackNumber = track.TrackNumber,
                ImageUrl = GetOptimalImage(track.Album?.Images),
                Images = track.Album?.Images.Select(img => new ImageDto
                {
                    Url = img.Url,
                    Height = img.Height,
                    Width = img.Width
                }).ToList() ?? new List<ImageDto>(),
                Popularity = track.Popularity,
                ExternalUrls = track.ExternalUrls != null ? new List<string> {track.ExternalUrls.Spotify} : null
            }).ToList();

            result.TotalResults += response.Tracks.Total;
        }

        return result;
    }
}