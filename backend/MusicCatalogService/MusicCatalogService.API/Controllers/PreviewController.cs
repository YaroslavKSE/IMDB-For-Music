using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.API.Models;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

[Route("api/v1/catalog/preview")]
public class PreviewController : BaseApiController
{
    private readonly IPreviewService _previewService;
    
    public PreviewController(
        IPreviewService previewService,
        ILogger<PreviewController> logger) 
        : base(logger)
    {
        _previewService = previewService;
    }
    
    [HttpGet("items")]
    [ProducesResponseType(typeof(MultiTypePreviewResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPreviewItems([FromQuery] string ids, [FromQuery] string types)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Item IDs query parameter is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        
        if (string.IsNullOrWhiteSpace(types))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Types parameter is required (e.g., 'track', 'album', 'artist', or a comma-separated combination)",
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
                Message = "At least one item ID is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (spotifyIds.Count > 50)  // Set a reasonable limit
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Maximum number of item IDs per request is 50",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        
        // Parse the requested types
        var typesList = types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLower())
            .ToList();
        
        // Validate the types
        var validTypes = new[] { "track", "album", "artist" };
        foreach (var type in typesList)
        {
            if (!validTypes.Contains(type))
            {
                return BadRequest(new ErrorResponse
                {
                    Message = $"Invalid type '{type}'. Valid types are: track, album, artist",
                    ErrorCode = ErrorCodes.ValidationError,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        try
        {
            _logger.LogInformation("Retrieving preview items for types: {Types}, IDs count: {Count}", 
                string.Join(", ", typesList), spotifyIds.Count);
            
            var result = await _previewService.GetMultiTypePreviewItemsAsync(spotifyIds, typesList);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preview items");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred while retrieving preview items",
                ErrorCode = ErrorCodes.InternalServerError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
    
    // This is required by BaseApiController, but not used in this controller
    [NonAction]
    public override Task<IActionResult> GetById(Guid id)
    {
        return Task.FromResult<IActionResult>(NotFound());
    }
}