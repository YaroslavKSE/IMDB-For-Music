using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Responses;

public class Auth0UserResponse
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}

