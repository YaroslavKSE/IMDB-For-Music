﻿using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string query, string type, int limit = 20, int offset = 0, string? market = null);
}