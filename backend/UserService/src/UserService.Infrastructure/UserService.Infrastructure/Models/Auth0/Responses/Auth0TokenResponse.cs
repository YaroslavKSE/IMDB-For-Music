using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Responses;

public class Auth0TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
