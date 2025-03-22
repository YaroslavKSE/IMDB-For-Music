namespace MusicCatalogService.Core.Models.Spotify;

public class SpotifySettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public int CacheExpirationMinutes { get; set; } = 60;
    public int RateLimitPerMinute { get; set; } = 160; // Spotify's rate limit is ~ 2,5 requests per second
}