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
            
            // Check if we can extract error_description
            if (jsonResponse.TryGetProperty("error_description", out var errorDescriptionElement))
            {
                return new Auth0Error
                {
                    Code = jsonResponse.TryGetProperty("error", out var errorElement) 
                        ? errorElement.GetString() 
                        : "auth0_error",
                    Message = errorDescriptionElement.GetString()
                };
            }
            // Fallback to checking if error is inside a nested JSON string
            else if (errorMessage.Contains("error_description"))
            {
                try
                {
                    // The error might be a JSON string that's itself inside a JSON string
                    // Try to parse it by removing escape characters
                    errorMessage = errorMessage.Replace("\\\"", "\"").Trim('"');
                    var nestedJson = JsonSerializer.Deserialize<JsonElement>(errorMessage);
                    
                    if (nestedJson.TryGetProperty("error_description", out var nestedErrorDescriptionElement))
                    {
                        return new Auth0Error
                        {
                            Code = nestedJson.TryGetProperty("error", out var nestedErrorElement) 
                                ? nestedErrorElement.GetString() 
                                : "auth0_error",
                            Message = nestedErrorDescriptionElement.GetString()
                        };
                    }
                }
                catch
                {
                    // If we can't parse the nested JSON, fall through to the default handling
                }
            }
        }
        catch
        {
            // If parsing fails, continue to the default handling
        }

        // Default fallback if we couldn't extract a clean error message
        return new Auth0Error
        {
            Code = "auth0_error",
            Message = errorMessage
        };
    }
}