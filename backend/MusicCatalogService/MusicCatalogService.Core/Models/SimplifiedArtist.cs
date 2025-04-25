using MongoDB.Bson.Serialization.Attributes;

namespace MusicCatalogService.Core.Models;

public class SimplifiedArtist
{ 
    [BsonElement("SpotifyId")]
    public string SpotifyId { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; }

    [BsonElement("SpotifyUrl")]
    public string SpotifyUrl { get; set; }
}