namespace MusicCatalogService.Core.DTOs;

public class PreviewItemDto
{
    public string SpotifyId { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string ArtistName { get; set; }
}

public class PreviewItemsResultDto
{
    public string Type { get; set; }
    public int Count => Items.Count;
    public List<PreviewItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for returning preview items of multiple types in a single response
/// </summary>
public class MultiTypePreviewResultDto
{
    public List<PreviewItemsResultDto> Results { get; set; } = new();
    
    public int TotalCount => Results.Sum(r => r.Count);
}