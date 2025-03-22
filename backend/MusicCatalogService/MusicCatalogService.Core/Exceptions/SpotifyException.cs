using System.Net;

namespace MusicCatalogService.Core.Exceptions;

// Base exception for all Spotify-related errors
public abstract class SpotifyException : Exception
{
    public SpotifyException(string message) : base(message)
    {
    }

    public SpotifyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

// Resource not found (404)
public class SpotifyResourceNotFoundException : SpotifyException
{
    public string ResourceId { get; }

    public SpotifyResourceNotFoundException(string message, string resourceId) 
        : base(message)
    {
        ResourceId = resourceId;
    }
}

// Authorization issues (401, 403)
public class SpotifyAuthorizationException : SpotifyException
{
    public SpotifyAuthorizationException(string message) 
        : base(message)
    {
    }
}

// Rate limiting (429)
public class SpotifyRateLimitException : SpotifyException
{
    public SpotifyRateLimitException(string message) 
        : base(message)
    {
    }
}

// Generic API exception
public class SpotifyApiException : SpotifyException
{
    public HttpStatusCode StatusCode { get; }

    public SpotifyApiException(string message, HttpStatusCode statusCode) 
        : base(message)
    {
        StatusCode = statusCode;
    }

    public SpotifyApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}