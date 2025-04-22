using MongoDB.Bson.Serialization.Attributes;

namespace MusicCatalogService.Core.Models;

public abstract class CatalogItemBase
{
    [BsonId] public Guid Id { get; set; }

    public string SpotifyId { get; set; }

    public string Name { get; set; }

    public string ArtistName { get; set; }

    public string ThumbnailUrl { get; set; }

    public int? Popularity { get; set; }

    public DateTime LastAccessed { get; set; }

    public DateTime CacheExpiresAt { get; set; }
}