using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Interfaces;

public interface ISpotifyTokenService
{
    Task<TokenResult> GetAccessTokenAsync();
}