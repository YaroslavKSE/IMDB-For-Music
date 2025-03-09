namespace MusicCatalogService.Core.Models.Spotify;

public class SpotifyTokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
}

public class SpotifyAlbumResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SpotifyArtistSimplified> Artists { get; set; }
    public List<SpotifyImage> Images { get; set; }
    public string ReleaseDate { get; set; }
    public int Popularity { get; set; }
    public SpotifyPagingObject<SpotifyTrackSimplified> Tracks { get; set; }
    public string AlbumType { get; set; }
}

public class SpotifyTrackResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SpotifyArtistSimplified> Artists { get; set; }
    public SpotifyAlbumSimplified Album { get; set; }
    public List<SpotifyImage> Images { get; set; }
    public int DurationMs { get; set; }
    public int Popularity { get; set; }
    public bool Explicit { get; set; }
}

public class SpotifyArtistResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SpotifyImage> Images { get; set; }
    public List<string> Genres { get; set; }
    public int Popularity { get; set; }
    public int FollowersCount { get; set; }
}

public class SpotifySearchResponse
{
    public SpotifyPagingObject<SpotifyAlbumSimplified> Albums { get; set; }
    public SpotifyPagingObject<SpotifyTrackSimplified> Tracks { get; set; }
    public SpotifyPagingObject<SpotifyArtistSimplified> Artists { get; set; }
}

public class SpotifyNewReleasesResponse
{
    public SpotifyPagingObject<SpotifyAlbumSimplified> Albums { get; set; }
}

public class SpotifyPagingObject<T>
{
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string Next { get; set; }
    public string Previous { get; set; }
    public List<T> Items { get; set; }
}

public class SpotifyImage
{
    public string Url { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}

public class SpotifyArtistSimplified
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class SpotifyAlbumSimplified
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SpotifyArtistSimplified> Artists { get; set; }
    public List<SpotifyImage> Images { get; set; }
    public string ReleaseDate { get; set; }
    public string AlbumType { get; set; }
}

public class SpotifyTrackSimplified
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<SpotifyArtistSimplified> Artists { get; set; }
    public int DurationMs { get; set; }
}
