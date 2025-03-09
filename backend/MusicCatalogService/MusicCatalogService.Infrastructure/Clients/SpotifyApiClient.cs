using System.Net.Http.Headers;
using System.Text.Json;
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

    public SpotifyApiClient(HttpClient httpClient, IOptions<SpotifySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri("https://api.spotify.com/v1/");
    }

    public async Task<SpotifyAlbumResponse?> GetAlbumAsync(string albumId)
    {
        await EnsureValidTokenAsync();
        var response = await _httpClient.GetAsync($"albums/{albumId}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SpotifyAlbumResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
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
        await EnsureValidTokenAsync();
        var response = await _httpClient.GetAsync($"search?q={Uri.EscapeDataString(query)}&type={type}&limit={limit}&offset={offset}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SpotifySearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

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

        var response = await tokenClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (tokenResponse != null)
        {
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Buffer of 60 seconds
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }
}