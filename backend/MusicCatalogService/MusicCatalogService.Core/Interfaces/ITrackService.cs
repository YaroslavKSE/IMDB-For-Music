using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface ITrackService
{
    Task<TrackDetailDto> GetTrackAsync(string spotifyId);
}