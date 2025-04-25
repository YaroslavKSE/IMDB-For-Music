using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicCatalogService.Core.Exceptions;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Infrastructure.Clients;

public class SpotifyApiClient : ISpotifyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISpotifyTokenService _tokenService;
    private readonly ILogger<SpotifyApiClient> _logger;

    public SpotifyApiClient(HttpClient httpClient, ILogger<SpotifyApiClient> logger, ISpotifyTokenService tokenService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tokenService = tokenService;
        _httpClient.BaseAddress = new Uri("https://api.spotify.com/v1/");
    }

    public async Task<SpotifyAlbumResponse?> GetAlbumAsync(string albumId)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for album {AlbumId}. Working with local data only.", albumId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            var response = await _httpClient.GetAsync($"albums/{albumId}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting album {AlbumId}. Response: {Response}",
                    response.StatusCode, albumId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, albumId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned {StatusCode}: {Content}", response.StatusCode, content);
            return JsonSerializer.Deserialize<SpotifyAlbumResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching album {AlbumId} from Spotify", albumId);
            throw new SpotifyApiException("An unexpected error occurred while communicating with Spotify", ex);
        }
    }

    public async Task<SpotifyPagingObject<SpotifyTrackSimplified>?> GetAlbumTracksAsync(string albumId, int limit = 20,
        int offset = 0, string? market = null)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for album tracks {AlbumId}. Working with local data only.", albumId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Build the request URL with query parameters
            var requestUrl = $"albums/{albumId}/tracks?limit={limit}&offset={offset}";

            // Add market parameter if provided
            if (!string.IsNullOrEmpty(market)) requestUrl += $"&market={market}";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Spotify API error: {StatusCode} when getting album tracks for {AlbumId}. Response: {Response}",
                    response.StatusCode, albumId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, albumId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned album tracks: {Content}", content);

            return JsonSerializer.Deserialize<SpotifyPagingObject<SpotifyTrackSimplified>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tracks for album {AlbumId} from Spotify", albumId);
            throw new SpotifyApiException("An unexpected error occurred while fetching album tracks", ex);
        }
    }

    public async Task<SpotifyTrackResponse?> GetTrackAsync(string trackId)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for track {TrackId}. Working with local data only.", trackId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            var response = await _httpClient.GetAsync($"tracks/{trackId}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting track {TrackId}. Response: {Response}",
                    response.StatusCode, trackId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, trackId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyTrackResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching track {TrackId} from Spotify", trackId);
            throw new SpotifyApiException("An unexpected error occurred while communicating with Spotify", ex);
        }
    }

    public async Task<SpotifyArtistResponse?> GetArtistAsync(string artistId)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for artist {ArtistId}. Working with local data only.", artistId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            var response = await _httpClient.GetAsync($"artists/{artistId}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting artist {ArtistId}. Response: {Response}",
                    response.StatusCode, artistId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, artistId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyArtistResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching artist {ArtistId} from Spotify", artistId);
            throw new SpotifyApiException("An unexpected error occurred while communicating with Spotify", ex);
        }
    }

    public async Task<SpotifyArtistAlbumsResponse?> GetArtistAlbumsAsync(string artistId, int limit = 20,
        int offset = 0, string? market = null, string? includeGroups = "album")
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for artist albums {ArtistId}. Working with local data only.", artistId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Build the request URL with query parameters
            var requestUrl = $"artists/{artistId}/albums?limit={limit}&offset={offset}";

            // Add market parameter if provided
            if (!string.IsNullOrEmpty(market)) requestUrl += $"&market={market}";

            // Add include_groups parameter if provided
            if (!string.IsNullOrEmpty(includeGroups)) requestUrl += $"&include_groups={includeGroups}";

            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Spotify API error: {StatusCode} when getting artist albums for {ArtistId}. Response: {Response}",
                    response.StatusCode, artistId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, artistId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned artist albums: {Content}", content);

            return JsonSerializer.Deserialize<SpotifyArtistAlbumsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching albums for artist {ArtistId} from Spotify", artistId);
            throw new SpotifyApiException("An unexpected error occurred while fetching artist albums", ex);
        }
    }

    public async Task<SpotifyArtistTopTracksResponse?> GetArtistTopTracksAsync(string artistId, string? market = null)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for artist top tracks {ArtistId}. Working with local data only.", artistId);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Build the request URL with query parameters
            // The market is required for this endpoint
            var requestUrl = $"artists/{artistId}/top-tracks?market={market ?? "US"}";

            var response = await _httpClient.GetAsync(requestUrl);

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Spotify API error: {StatusCode} when getting artist top tracks for {ArtistId}. Response: {Response}",
                    response.StatusCode, artistId, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, artistId),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned artist top tracks: {Content}", content);

            return JsonSerializer.Deserialize<SpotifyArtistTopTracksResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top tracks for artist {ArtistId} from Spotify", artistId);
            throw new SpotifyApiException("An unexpected error occurred while fetching artist top tracks", ex);
        }
    }

    public async Task<SpotifySearchResponse?> SearchAsync(string query, string type, int limit = 20, int offset = 0)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for search query '{Query}'. Working with local data only.", query);
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Encode the query and build the request URL
            var encodedQuery = Uri.EscapeDataString(query);
            var requestUrl = $"search?q={encodedQuery}&type={type}&limit={limit}&offset={offset}";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when searching. Query: {Query}. Response: {Response}",
                    response.StatusCode, query, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, query),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify search API returned {StatusCode}: {Content}", response.StatusCode, content);

            return JsonSerializer.Deserialize<SpotifySearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Spotify. Query: {Query}, Type: {Type}", query, type);
            throw new SpotifyApiException($"An unexpected error occurred while searching Spotify", ex);
        }
    }

    public async Task<SpotifyNewReleasesResponse?> GetNewReleasesAsync(int limit = 20, int offset = 0)
    {
        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for new releases. Working with local data only.");
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            var response = await _httpClient.GetAsync($"browse/new-releases?limit={limit}&offset={offset}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting new releases. Response: {Response}",
                    response.StatusCode, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message,
                        "new-releases"),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyNewReleasesResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching new releases from Spotify");
            throw new SpotifyApiException("An unexpected error occurred while fetching new releases", ex);
        }
    }

    public async Task<SpotifyMultipleAlbumsResponse?> GetMultipleAlbumsAsync(IEnumerable<string> albumIds)
    {
        if (albumIds == null || !albumIds.Any())
            throw new ArgumentException("Album IDs cannot be null or empty", nameof(albumIds));

        // Spotify API limit is 20 IDs per request
        if (albumIds.Count() > 20)
            throw new ArgumentException("Maximum number of album IDs per request is 20", nameof(albumIds));

        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for multiple albums. Working with local data only.");
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Join album IDs with comma
            var idsParameter = string.Join(",", albumIds);
            var response = await _httpClient.GetAsync($"albums?ids={idsParameter}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting multiple albums. Response: {Response}",
                    response.StatusCode, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, idsParameter),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned multiple albums response: {Content}", content);
            return JsonSerializer.Deserialize<SpotifyMultipleAlbumsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (ArgumentException)
        {
            // Let argument exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching multiple albums from Spotify");
            throw new SpotifyApiException("An unexpected error occurred while fetching multiple albums", ex);
        }
    }

    public async Task<SpotifyMultipleTracksResponse?> GetMultipleTracksAsync(IEnumerable<string> trackIds)
    {
        if (trackIds == null || !trackIds.Any())
            throw new ArgumentException("Track IDs cannot be null or empty", nameof(trackIds));

        // Spotify API limit is 50 IDs per request
        if (trackIds.Count() > 50)
            throw new ArgumentException("Maximum number of track IDs per request is 50", nameof(trackIds));

        try
        {
            var tokenResult = await _tokenService.GetAccessTokenAsync();
            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Unable to get Spotify token for multiple albums. Working with local data only.");
                return null;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Join track IDs with comma
            var idsParameter = string.Join(",", trackIds);
            var response = await _httpClient.GetAsync($"tracks?ids={idsParameter}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting multiple tracks. Response: {Response}",
                    response.StatusCode, errorContent);

                // Parse Spotify error response
                var spotifyError = ParseSpotifyError(errorContent, response.StatusCode);

                throw response.StatusCode switch
                {
                    // Throw appropriate exception based on status code
                    HttpStatusCode.NotFound => new SpotifyResourceNotFoundException(spotifyError.Message, idsParameter),
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new SpotifyAuthorizationException(
                        spotifyError.Message),
                    HttpStatusCode.TooManyRequests => new SpotifyRateLimitException(spotifyError.Message),
                    _ => new SpotifyApiException(spotifyError.Message, response.StatusCode)
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned multiple tracks response: {Content}", content);
            return JsonSerializer.Deserialize<SpotifyMultipleTracksResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (SpotifyException)
        {
            // Let the custom exceptions propagate
            throw;
        }
        catch (ArgumentException)
        {
            // Let argument exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching multiple tracks from Spotify");
            throw new SpotifyApiException("An unexpected error occurred while fetching multiple tracks", ex);
        }
    }

    private SpotifyError ParseSpotifyError(string errorContent, HttpStatusCode statusCode)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<SpotifyErrorResponse>(errorContent,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            if (errorResponse?.Error != null)
                return new SpotifyError
                {
                    Status = errorResponse.Error.Status,
                    Message = errorResponse.Error.Message
                };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Spotify error response: {ErrorContent}", errorContent);
        }

        // Default error if parsing fails
        return new SpotifyError
        {
            Status = (int) statusCode,
            Message = $"Spotify API error: {statusCode}"
        };
    }
}

// Helper class to parse Spotify error responses
public class SpotifyErrorResponse
{
    public SpotifyError Error { get; set; }
}

public class SpotifyError
{
    public int Status { get; set; }
    public string Message { get; set; }
}