namespace MusicCatalogService.Core.Responses;

public class ErrorResponse
{
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public string TraceId { get; set; }
    public object Details { get; set; }
}

// Specific error response details
public class SpotifyErrorDetails
{
    public string SpotifyId { get; set; }
    public int StatusCode { get; set; }
}

public static class ErrorCodes
{
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string SpotifyApiError = "SPOTIFY_API_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string AuthorizationError = "AUTHORIZATION_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}