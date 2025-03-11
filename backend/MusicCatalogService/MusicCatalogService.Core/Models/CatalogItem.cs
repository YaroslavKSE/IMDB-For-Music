using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MusicCatalogService.Core.Models;

public class CatalogItem
{
    [BsonId]
    public Guid Id { get; set; }
    public string SpotifyId { get; set; }
    public string Type { get; set; } // "album", "track", etc.
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public string ThumbnailUrl { get; set; }
    public int? Popularity { get; set; }
    public DateTime LastAccessed { get; set; }
    public DateTime CacheExpiresAt { get; set; }
    
    // Additional fields for tracks
    public int? DurationMs { get; set; }
    public bool? IsExplicit { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public string Isrc { get; set; }
    public string PreviewUrl { get; set; }
    public string AlbumId { get; set; }
    public string AlbumName { get; set; }
    
    // JSON serialized data
    public string RawData { get; set; }
}