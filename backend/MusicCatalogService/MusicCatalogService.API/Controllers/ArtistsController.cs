using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.API.Models;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

[Route("api/v1/catalog/artists")]
public class ArtistsController : BaseApiController
{
    private readonly IArtistService _artistService;
    
    public ArtistsController(
        IArtistService artistService,
        ILogger<ArtistsController> logger) 
        : base(logger)
    {
        _artistService = artistService;
    }
    
    [HttpGet("{catalogId:guid}")]
    [ProducesResponseType(typeof(ArtistDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public override async Task<IActionResult> GetById(Guid catalogId)
    {
        return await ExecuteApiActionAsync(
            () => _artistService.GetArtistByCatalogIdAsync(catalogId),
            $"Error retrieving artist with catalog ID: {catalogId}");
    }
    
    [HttpGet("spotify/{spotifyId}")]
    [ProducesResponseType(typeof(ArtistDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBySpotifyId(string spotifyId)
    {
        return await ExecuteApiActionAsync(
            () => _artistService.GetArtistAsync(spotifyId),
            $"Error retrieving artist with Spotify ID: {spotifyId}",
            spotifyId);
    }

    [HttpGet("spotify/{spotifyId}/albums")]
    [ProducesResponseType(typeof(ArtistAlbumsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetArtistAlbums(
        string spotifyId,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] string? market = null)
    {
        // Ensure limit is within acceptable range
        limit = Math.Clamp(limit, 1, 50);

        return await ExecuteApiActionAsync(
            () => _artistService.GetArtistAlbumsAsync(spotifyId, limit, offset, market),
            $"Error retrieving albums for artist with Spotify ID: {spotifyId}",
            spotifyId);
    }

    [HttpGet("spotify/{spotifyId}/top-tracks")]
    [ProducesResponseType(typeof(ArtistTopTracksResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetArtistTopTracks(
        string spotifyId,
        [FromQuery] string? market = null)
    {
        return await ExecuteApiActionAsync(
            () => _artistService.GetArtistTopTracksAsync(spotifyId, market),
            $"Error retrieving top tracks for artist with Spotify ID: {spotifyId}",
            spotifyId);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(SaveItemSuccessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveArtist([FromBody] SaveMusicItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SpotifyId))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Spotify ID is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        
        return await ExecuteCreateActionAsync(
            () => _artistService.SaveArtistAsync(request.SpotifyId),
            $"Error saving artist with Spotify ID: {request.SpotifyId}",
            request.SpotifyId,
            artist => new SaveItemSuccessResponse(
                artist.CatalogItemId, 
                $"Artist with ID {request.SpotifyId} was successfully saved"),
            artist => new { catalogId = artist.CatalogItemId }
        );
    }
}