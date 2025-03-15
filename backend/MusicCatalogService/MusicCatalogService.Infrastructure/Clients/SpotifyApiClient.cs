using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;

namespace MusicCatalogService.Infrastructure.Clients;

public class SpotifyApiClient : ISpotifyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SpotifySettings _settings;
    private string _accessToken;
    private DateTime _tokenExpiryTime = DateTime.MinValue;
    private readonly ILogger<SpotifyApiClient> _logger;

    public SpotifyApiClient(HttpClient httpClient, IOptions<SpotifySettings> settings, ILogger<SpotifyApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri("https://api.spotify.com/v1/");
    }

    public async Task<SpotifyAlbumResponse?> GetAlbumAsync(string albumId)
    {
        try
        {
            await EnsureValidTokenAsync();
            var response = await _httpClient.GetAsync($"albums/{albumId}");

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when getting album {AlbumId}. Response: {Response}",
                    response.StatusCode, albumId, errorContent);

                throw new Exception($"Spotify API returned {response.StatusCode}. Details: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify API returned {StatusCode}: {Content}", response.StatusCode, content);
            return JsonSerializer.Deserialize<SpotifyAlbumResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching album {AlbumId} from Spotify", albumId);
            throw;
        }
    }

    public async Task<SpotifyTrackResponse?> GetTrackAsync(string trackId)
    {
        await EnsureValidTokenAsync();
        var response = await _httpClient.GetAsync($"tracks/{trackId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SpotifyTrackResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<SpotifyArtistResponse?> GetArtistAsync(string artistId)
    {
        await EnsureValidTokenAsync();
        var response = await _httpClient.GetAsync($"artists/{artistId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SpotifyArtistResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<SpotifySearchResponse?> SearchAsync(string query, string type, int limit = 20, int offset = 0)
    {
        try
        {
            await EnsureValidTokenAsync();
        
            // Encode the query and build the request URL
            var encodedQuery = Uri.EscapeDataString(query);
            var requestUrl = $"search?q={encodedQuery}&type={type}&limit={limit}&offset={offset}";
        
            var response = await _httpClient.GetAsync(requestUrl);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API error: {StatusCode} when searching. Query: {Query}. Response: {Response}",
                    response.StatusCode, query, errorContent);
            
                throw new Exception($"Spotify API returned {response.StatusCode}. Details: {errorContent}");
            }
        
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Spotify search API returned {StatusCode}: {Content}", response.StatusCode, content);
        
            return JsonSerializer.Deserialize<SpotifySearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Spotify. Query: {Query}, Type: {Type}", query, type);
            throw;
        }
    }
    public async Task<SpotifyNewReleasesResponse?> GetNewReleasesAsync(int limit = 20, int offset = 0)
    {
        await EnsureValidTokenAsync();
        var response = await _httpClient.GetAsync($"browse/new-releases?limit={limit}&offset={offset}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SpotifyNewReleasesResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task EnsureValidTokenAsync()
    {
        if (DateTime.UtcNow < _tokenExpiryTime && !string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogDebug("Using existing token, expires in {TimeRemaining} seconds",
                (_tokenExpiryTime - DateTime.UtcNow).TotalSeconds);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

        _logger.LogInformation("Requesting new Spotify access token");

        // Request new token
        using var tokenClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", _settings.ClientId},
                {"client_secret", _settings.ClientSecret}
            })
        };

        try
        {
            var response = await tokenClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Spotify token. Status: {Status}, Response: {Response}",
                    response.StatusCode, responseContent);
                throw new Exception($"Failed to get Spotify token: {responseContent}");
            }

            _logger.LogInformation("Successfully obtained new Spotify token");

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Buffer of 60 seconds
                _logger.LogDebug("Token will expire at {ExpiryTime}", _tokenExpiryTime);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining Spotify token");
            throw;
        }
    }
}