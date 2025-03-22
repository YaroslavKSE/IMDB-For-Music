using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicCatalogService.Core.Exceptions;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Services;

public class SpotifyTokenService : ISpotifyTokenService
{
    private readonly SpotifySettings _settings;
    private readonly ILogger<SpotifyTokenService> _logger;
    private string _accessToken;
    private DateTime _tokenExpiryTime = DateTime.MinValue;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public SpotifyTokenService(IOptions<SpotifySettings> settings, ILogger<SpotifyTokenService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Check if token is valid
        if (DateTime.UtcNow < _tokenExpiryTime && !string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogDebug("Using existing token, expires in {TimeRemaining} seconds",
                (_tokenExpiryTime - DateTime.UtcNow).TotalSeconds);
            return _accessToken;
        }

        // Prevent concurrent token requests
        await _semaphore.WaitAsync();
        try
        {
            // Double-check in case another thread already refreshed the token
            if (DateTime.UtcNow < _tokenExpiryTime && !string.IsNullOrEmpty(_accessToken))
            {
                return _accessToken;
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

            var response = await tokenClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Spotify token. Status: {Status}, Response: {Response}",
                    response.StatusCode, responseContent);
                throw new SpotifyAuthorizationException($"Failed to get Spotify token: {responseContent}");
            }

            _logger.LogInformation("Successfully obtained new Spotify token");

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
                _logger.LogDebug("Token will expire at {ExpiryTime}", _tokenExpiryTime);
            }

            return _accessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}