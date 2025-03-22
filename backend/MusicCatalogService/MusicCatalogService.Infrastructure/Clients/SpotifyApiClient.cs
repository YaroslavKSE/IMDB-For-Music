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
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

    public async Task<SpotifyTrackResponse?> GetTrackAsync(string trackId)
    {
        try
        {
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

    public async Task<SpotifySearchResponse?> SearchAsync(string query, string type, int limit = 20, int offset = 0)
    {
        try
        {
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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