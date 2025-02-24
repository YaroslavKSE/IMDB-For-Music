using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Responses;

public class Auth0TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    
    // Optional properties that may not be returned in all grant types
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}