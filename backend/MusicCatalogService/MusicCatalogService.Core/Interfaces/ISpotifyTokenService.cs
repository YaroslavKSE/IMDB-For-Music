namespace MusicCatalogService.Core.Interfaces;

public interface ISpotifyTokenService
{
    Task<string> GetAccessTokenAsync();
}