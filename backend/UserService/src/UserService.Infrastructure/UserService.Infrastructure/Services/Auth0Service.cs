using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Configuration;
using UserService.Infrastructure.Models.Auth0.Exceptions;
using UserService.Infrastructure.Models.Auth0.Requests;
using UserService.Infrastructure.Models.Auth0.Responses;

namespace UserService.Infrastructure.Services;

public class Auth0Service : IAuth0Service
{
    private readonly HttpClient _httpClient;
    private readonly Auth0Settings _settings;
    private readonly ILogger<Auth0Service> _logger;
    private string _managementApiToken;
    private DateTime _tokenExpirationTime;

    public Auth0Service(
        HttpClient httpClient,
        IOptions<Auth0Settings> settings,
        ILogger<Auth0Service> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> CreateUserAsync(string email, string password)
    {
        try
        {
            await EnsureManagementApiToken();

            var createUserRequest = new Auth0CreateUserRequest
            {
                Email = email,
                Password = password,
                Connection = "Username-Password-Authentication",
                VerifyEmail = true
            };

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _managementApiToken);

            var response = await _httpClient.PostAsJsonAsync(
                $"https://{_settings.Domain}/api/v2/users",
                createUserRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create Auth0 user. Status: {Status}, Error: {Error}", 
                    response.StatusCode, error);
                throw new Auth0Exception(error);
            }

            var result = await response.Content.ReadFromJsonAsync<Auth0UserResponse>();
            return result.UserId;
        }
        catch (Exception ex) when (ex is not Auth0Exception)
        {
            _logger.LogError(ex, "Error creating Auth0 user for email {Email}", email);
            throw new Auth0Exception("Failed to create Auth0 user", ex);
        }
    }
    
    public async Task<AuthTokenResponse> LoginAsync(string email, string password)
    {
        try
        {
            var tokenRequest = new Auth0PasswordTokenRequest
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret,
                Audience = _settings.ManagementApiAudience,
                Username = email,
                Password = password,
                Realm = "Username-Password-Authentication" 
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://{_settings.Domain}/oauth/token",
                tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Login failed. Status: {Status}, Error: {Error}", 
                    response.StatusCode, error);
                throw new Auth0Exception(error);
            }

            var auth0Response = await response.Content.ReadFromJsonAsync<Auth0TokenResponse>();
            
            // Map to application layer DTO
            return new AuthTokenResponse
            {
                AccessToken = auth0Response.AccessToken,
                RefreshToken = auth0Response.RefreshToken,
                IdToken = auth0Response.IdToken,
                ExpiresIn = auth0Response.ExpiresIn,
                TokenType = auth0Response.TokenType
            };
        }
        catch (Exception ex) when (ex is not Auth0Exception)
        {
            _logger.LogError(ex, "Error during login for email {Email}", email);
            throw new Auth0Exception("Login failed", ex);
        }
    }

    private async Task EnsureManagementApiToken()
    {
        if (_managementApiToken != null && DateTime.UtcNow < _tokenExpirationTime)
        {
            return;
        }

        var tokenRequest = new Auth0TokenRequest
        {
            ClientId = _settings.ClientId,
            ClientSecret = _settings.ClientSecret,
            Audience = _settings.ManagementApiAudience,
            GrantType = "client_credentials"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://{_settings.Domain}/oauth/token",
            tokenRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to obtain management API token. Status: {Status}, Error: {Error}",
                response.StatusCode, error);
            throw new Auth0Exception("Failed to obtain management API token");
        }

        var result = await response.Content.ReadFromJsonAsync<Auth0TokenResponse>();
        _managementApiToken = result.AccessToken;
        _tokenExpirationTime = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 60); // Buffer of 60 seconds
    }
}