using System.Collections.Generic;
using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.DTOs;

public class MultipleAlbumsOverviewDto
{
    public List<AlbumSummaryDto> Albums { get; set; } = new();
    public int Count => Albums?.Count ?? 0;
}

public class MultipleTracksOverviewDto
{
    public List<TrackSummaryDto> Tracks { get; set; } = new();
    public int Count => Tracks?.Count ?? 0;
}