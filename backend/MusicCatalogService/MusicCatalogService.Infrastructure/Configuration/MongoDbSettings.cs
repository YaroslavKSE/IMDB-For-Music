namespace MusicCatalogService.Infrastructure.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string AlbumsCollectionName { get; set; } = "Albums";
    public string TracksCollectionName { get; set; } = "Tracks";
    public string ArtistsCollectionName { get; set; } = "Artists";
    public string SpotifyCacheCollectionName { get; set; } = "SpotifyCache";
}