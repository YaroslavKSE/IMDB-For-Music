using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.API.Models;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

[Route("api/v1/catalog/tracks")]
public class TracksController : BaseApiController
{
    private readonly ITrackService _trackService;
    
    public TracksController(
        ITrackService trackService,
        ILogger<TracksController> logger) 
        : base(logger)
    {
        _trackService = trackService;
    }
    
    [HttpGet("{catalogId:guid}")]
    [ProducesResponseType(typeof(TrackDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public override async Task<IActionResult> GetById(Guid catalogId)
    {
        return await ExecuteApiActionAsync(
            () => _trackService.GetTrackByCatalogIdAsync(catalogId),
            $"Error retrieving track with catalog ID: {catalogId}");
    }
    
    [HttpGet("spotify/{spotifyId}")]
    [ProducesResponseType(typeof(TrackDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBySpotifyId(string spotifyId)
    {
        return await ExecuteApiActionAsync(
            () => _trackService.GetTrackAsync(spotifyId),
            $"Error retrieving track with Spotify ID: {spotifyId}",
            spotifyId);
    }

    [HttpGet("spotify")]
    [ProducesResponseType(typeof(MultipleTracksResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMultipleTracksBySpotifyIds([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Track IDs query parameter is required",
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
                Message = "At least one track ID is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (spotifyIds.Count > 50)
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Maximum number of track IDs per request is 50",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            _logger.LogInformation("Retrieving batch of {Count} tracks via GET", spotifyIds.Count);
            var result = await _trackService.GetMultipleTracksAsync(spotifyIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple tracks via GET");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred while retrieving tracks",
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
    public async Task<IActionResult> SaveTrack([FromBody] SaveMusicItemRequest request)
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
            () => _trackService.SaveTrackAsync(request.SpotifyId),
            $"Error saving track with Spotify ID: {request.SpotifyId}",
            request.SpotifyId,
            track => new SaveItemSuccessResponse(
                track.CatalogItemId, 
                $"Track with ID {request.SpotifyId} was successfully saved"),
            track => new { catalogId = track.CatalogItemId }
        );
    }
}