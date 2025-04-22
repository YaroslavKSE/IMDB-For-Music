using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface IPreviewService
{
    /// <summary>
    /// Get lightweight preview information for multiple items of different types in a single request
    /// </summary>
    /// <param name="spotifyIds">List of Spotify IDs to look up</param>
    /// <param name="types">List of types to check (e.g., "track", "album", "artist")</param>
    /// <returns>Categorized preview items by type</returns>
    Task<MultiTypePreviewResultDto> GetMultiTypePreviewItemsAsync(IEnumerable<string> spotifyIds, IEnumerable<string> types);

}