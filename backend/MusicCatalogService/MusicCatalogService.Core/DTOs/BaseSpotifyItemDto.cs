using System.Text.Json.Serialization;

namespace MusicCatalogService.Core.DTOs;

public abstract class BaseSpotifyItemDto
{
    public Guid CatalogItemId { get; set; }
    public string SpotifyId { get; set; }
    public string Name { get; set; }

    // Primary image URL for quick access
    public string ImageUrl { get; set; }

    // Collection of images in different sizes
    public List<ImageDto> Images { get; set; } = new();

    public int? Popularity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> ExternalUrls { get; set; }
}