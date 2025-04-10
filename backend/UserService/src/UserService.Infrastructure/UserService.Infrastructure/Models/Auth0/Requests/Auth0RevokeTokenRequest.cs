using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Requests;

public class Auth0RevokeTokenRequest
{
    [JsonPropertyName("client_id")] public string ClientId { get; set; }

    [JsonPropertyName("client_secret")] public string ClientSecret { get; set; }

    [JsonPropertyName("token")] public string Token { get; set; }
}