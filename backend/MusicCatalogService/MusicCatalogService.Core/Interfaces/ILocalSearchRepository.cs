using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface ILocalSearchRepository
{
    Task<SearchResultDto> SearchLocalCatalogAsync(
        string query, 
        string type, 
        int limit = 20, 
        int offset = 0);
}