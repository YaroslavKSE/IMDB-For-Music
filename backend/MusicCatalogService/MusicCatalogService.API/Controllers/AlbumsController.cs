using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.API.Models;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

[Route("api/v1/catalog/albums")]
public class AlbumsController : BaseApiController
{
    private readonly IAlbumService _albumService;
    
    public AlbumsController(
        IAlbumService albumService,
        ILogger<AlbumsController> logger) 
        : base(logger)
    {
        _albumService = albumService;
    }
    
    [HttpGet("{catalogId:guid}")]
    [ProducesResponseType(typeof(AlbumDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public override async Task<IActionResult> GetById(Guid catalogId)
    {
        return await ExecuteApiActionAsync(
            () => _albumService.GetAlbumByCatalogIdAsync(catalogId),
            $"Error retrieving album with catalog ID: {catalogId}");
    }
    
    [HttpGet("spotify/{spotifyId}")]
    [ProducesResponseType(typeof(AlbumDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBySpotifyId(string spotifyId)
    {
        return await ExecuteApiActionAsync(
            () => _albumService.GetAlbumAsync(spotifyId),
            $"Error retrieving album with Spotify ID: {spotifyId}",
            spotifyId);
    }

    [HttpGet("spotify")]
    [ProducesResponseType(typeof(MultipleAlbumsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMultipleAlbumsBySpotifyIds([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Album IDs query parameter is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        // Parse the comma-separated IDs
        var spotifyIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        if (!spotifyIds.Any())
        {
            return BadRequest(new ErrorResponse
            {
                Message = "At least one album ID is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (spotifyIds.Count > 20)
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Maximum number of album IDs per request is 20",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            _logger.LogInformation("Retrieving batch of {Count} album overviews via GET", spotifyIds.Count);
            var result = await _albumService.GetMultipleAlbumsOverviewAsync(spotifyIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple album overviews via GET");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred while retrieving albums",
                ErrorCode = ErrorCodes.InternalServerError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(SaveItemSuccessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveAlbum([FromBody] SaveMusicItemRequest request)
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
            () => _albumService.SaveAlbumAsync(request.SpotifyId),
            $"Error saving album with Spotify ID: {request.SpotifyId}",
            request.SpotifyId,
            album => new SaveItemSuccessResponse(
                album.CatalogItemId, 
                $"Album with ID {request.SpotifyId} was successfully saved"),
            album => new { catalogId = album.CatalogItemId }
        );
    }
}