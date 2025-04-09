using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface ITrackService
{
    // Get track from Spotify and cache
    Task<TrackDetailDto> GetTrackAsync(string spotifyId);
    
    // Get track from internal database
    Task<TrackDetailDto> GetTrackByCatalogIdAsync(Guid catalogId);
    
    // Save track into internal database
    Task<TrackDetailDto> SaveTrackAsync(string spotifyId);

    // New method to get multiple tracks with simplified overview
    Task<MultipleTracksOverviewDto> GetMultipleTracksOverviewAsync(IEnumerable<string> spotifyIds);
}