using System.Text.Json;

namespace UserService.Infrastructure.Models.Auth0.Errors;

public class Auth0Error
{
    public string? Code { get; set; }
    public string? Message { get; set; }

    public static Auth0Error Parse(string errorMessage)
    {
        try
        {
            // Try to parse JSON error response from Auth0
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(errorMessage);
            
            if (jsonResponse.TryGetProperty("error", out var errorElement) &&
                jsonResponse.TryGetProperty("message", out var messageElement))
            {
                return new Auth0Error
                {
                    Code = errorElement.GetString(),
                    Message = messageElement.GetString()
                };
            }
        }
        catch
        {
            // If parsing fails, return the raw message
        }

        return new Auth0Error
        {
            Code = "auth0_error",
            Message = errorMessage
        };
    }
}