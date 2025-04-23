using MongoDB.Bson.Serialization.Attributes;

namespace MusicCatalogService.Core.Models;

public class SimplifiedArtist
{
    [BsonElement("_id")] public string Id { get; set; }

    public string Name { get; set; }

    public string SpotifyUrl { get; set; }
}