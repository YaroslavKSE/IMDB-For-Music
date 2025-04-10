using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Requests;

public class Auth0CreateUserRequest
{
    [JsonPropertyName("email")] public string Email { get; set; }

    [JsonPropertyName("password")] public string Password { get; set; }

    [JsonPropertyName("connection")] public string Connection { get; set; }

    [JsonPropertyName("verify_email")] public bool VerifyEmail { get; set; }
}