using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Requests;

public class Auth0PasswordTokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("audience")]
    public string Audience { get; set; }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "password";
    
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "openid profile email offline_access";
    
    [JsonPropertyName("realm")]
    public string Realm { get; set; }
}