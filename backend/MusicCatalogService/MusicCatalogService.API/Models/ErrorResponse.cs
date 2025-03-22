using MusicCatalogService.Core.Responses;

namespace MusicCatalogService.API.Models;

public class ErrorResponse
{
    public string Message { get; set; }
    public string TraceId { get; set; }
    public string ErrorCode { get; set; }
    public SpotifyErrorDetails Details { get; set; }
}

public class SaveMusicItemRequest
{
    public string SpotifyId { get; set; }
}
