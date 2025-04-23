using MongoDB.Bson.Serialization.Attributes;

namespace MusicCatalogService.Core.Models;

public class SimplifiedArtist
{ 
    [BsonElement("Id")]
    public string SpotifyId { get; set; }

    public string Name { get; set; }

    public string SpotifyUrl { get; set; }
}