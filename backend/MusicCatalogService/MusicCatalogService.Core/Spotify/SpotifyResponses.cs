using System.Text.Json.Serialization;

namespace MusicCatalogService.Core.Spotify;

public class SpotifyTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

public class SpotifyAlbumResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("artists")] public List<SpotifyArtistSimplified> Artists { get; set; }

    [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }

    [JsonPropertyName("release_date")] public string ReleaseDate { get; set; }

    [JsonPropertyName("popularity")] public int Popularity { get; set; }

    [JsonPropertyName("tracks")] public SpotifyPagingObject<SpotifyTrackSimplified> Tracks { get; set; }

    [JsonPropertyName("album_type")] public string AlbumType { get; set; }

    [JsonPropertyName("total_tracks")] public int? TotalTracks { get; set; }

    // Adding missing properties
    [JsonPropertyName("release_date_precision")]
    public string ReleaseDatePrecision { get; set; }

    [JsonPropertyName("label")] public string Label { get; set; }

    [JsonPropertyName("copyrights")] public List<SpotifyCopyright> Copyrights { get; set; }

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }

    // Note: genres is deprecated and always returns an empty array according to Spotify API documentation
    // [JsonPropertyName("genres")] public List<string> Genres { get; set; }

    // Helper property to get a single copyright statement
    [JsonIgnore] public string Copyright => Copyrights?.FirstOrDefault()?.Text;
}

public class SpotifyCopyright
{
    [JsonPropertyName("text")] public string Text { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }
}

public class SpotifyTrackResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("artists")] public List<SpotifyArtistSimplified> Artists { get; set; }

    [JsonPropertyName("album")] public SpotifyAlbumSimplified Album { get; set; }

    [JsonPropertyName("duration_ms")] public int DurationMs { get; set; }

    [JsonPropertyName("popularity")] public int Popularity { get; set; }

    [JsonPropertyName("explicit")] public bool Explicit { get; set; }

    [JsonPropertyName("preview_url")] public string PreviewUrl { get; set; }

    [JsonPropertyName("track_number")] public int TrackNumber { get; set; }

    [JsonPropertyName("disc_number")] public int DiscNumber { get; set; }

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }

    [JsonPropertyName("external_ids")] public ExternalIds ExternalIds { get; set; }
}


public class SpotifyArtistResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }

    [JsonPropertyName("popularity")] public int Popularity { get; set; }

    [JsonPropertyName("followers")] public Followers Followers { get; set; }

    [JsonPropertyName("genres")] public List<string> Genres { get; set; }

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }

    // Helper property to simplify access to followers count
    [JsonIgnore] public int FollowersCount => Followers?.Total ?? 0;
}

public class SpotifyArtistAlbumsResponse
{
    [JsonPropertyName("items")]
    public List<SpotifyAlbumSimplified> Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("next")]
    public string Next { get; set; }

    [JsonPropertyName("previous")]
    public string Previous { get; set; }
}

public class SpotifyArtistTopTracksResponse
{
    [JsonPropertyName("tracks")]
    public List<SpotifyTrackResponse> Tracks { get; set; }
}

public class Followers
{
    [JsonPropertyName("href")] public string Href { get; set; }

    [JsonPropertyName("total")] public int Total { get; set; }
}

public class ExternalUrls
{
    [JsonPropertyName("spotify")] public string Spotify { get; set; }
}

public class ExternalIds
{
    [JsonPropertyName("isrc")] public string Isrc { get; set; }
}

public class SpotifySearchResponse
{
    [JsonPropertyName("albums")] public SpotifyPagingObject<SpotifyAlbumSimplified> Albums { get; set; }

    [JsonPropertyName("tracks")] public SpotifyPagingObject<SpotifyTrackSimplified> Tracks { get; set; }

    [JsonPropertyName("artists")] public SpotifyPagingObject<SpotifyArtistSimplified> Artists { get; set; }
}

public class SpotifyNewReleasesResponse
{
    [JsonPropertyName("albums")] public SpotifyPagingObject<SpotifyAlbumSimplified> Albums { get; set; }
}

public class SpotifyPagingObject<T>
{
    [JsonPropertyName("total")] public int Total { get; set; }

    [JsonPropertyName("limit")] public int Limit { get; set; }

    [JsonPropertyName("offset")] public int Offset { get; set; }

    [JsonPropertyName("next")] public string Next { get; set; }

    [JsonPropertyName("previous")] public string Previous { get; set; }

    [JsonPropertyName("items")] public List<T> Items { get; set; }
}

public class SpotifyImage
{
    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("width")] public int Width { get; set; }
}

public class SpotifyArtistSimplified
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }

    [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }

    [JsonPropertyName("popularity")] public int Popularity { get; set; }
}

public class SpotifyAlbumSimplified
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("artists")]
    public List<SpotifyArtistSimplified> Artists { get; set; }
    
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; }
    
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }
    
    [JsonPropertyName("album_type")]
    public string AlbumType { get; set; }
    
    [JsonPropertyName("total_tracks")]
    public int? TotalTracks { get; set; }
    
    [JsonPropertyName("external_urls")]
    public ExternalUrls ExternalUrls { get; set; }
}

public class SpotifyTrackSimplified
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("artists")] public List<SpotifyArtistSimplified> Artists { get; set; }

    [JsonPropertyName("duration_ms")] public int DurationMs { get; set; }

    [JsonPropertyName("track_number")] public int TrackNumber { get; set; }

    [JsonPropertyName("explicit")] public bool Explicit { get; set; }

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; }

    // Add these missing properties
    [JsonPropertyName("album")] public SpotifyAlbumSimplified Album { get; set; }

    [JsonPropertyName("popularity")] public int Popularity { get; set; }
}