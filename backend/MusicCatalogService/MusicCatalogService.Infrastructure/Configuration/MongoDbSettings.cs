namespace MusicCatalogService.Infrastructure.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CatalogItemsCollectionName { get; set; } = "CatalogItems";
    public string SpotifyCacheCollectionName { get; set; } = "SpotifyCache";
}