using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Exceptions;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

[Route("api/v1/catalog/search")]
public class SearchController : BaseApiController
{
    private readonly ISearchService _searchService;
    
    public SearchController(
        ISearchService searchService,
        ILogger<SearchController> logger) 
        : base(logger)
    {
        _searchService = searchService;
    }
    
    // This is required by the base class but not used in this controller
    public override Task<IActionResult> GetById(Guid id)
    {
        return Task.FromResult<IActionResult>(NotFound());
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string q, 
        [FromQuery] string type,
        [FromQuery] int limit = 20, 
        [FromQuery] int offset = 0, 
        [FromQuery] string? market = null)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Search query (q) is required",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        
        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Search type is required. Valid types: album, artist, track",
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        
        // Validate type parameter
        var validTypes = new[] { "album", "artist", "track" };
        var requestedTypes = type.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var requestedType in requestedTypes)
        {
            if (!validTypes.Contains(requestedType.Trim().ToLower()))
            {
                return BadRequest(new ErrorResponse
                {
                    Message = $"Invalid search type: {requestedType}. Valid types: album, artist, track",
                    ErrorCode = ErrorCodes.ValidationError,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
        
        try
        {
            _logger.LogInformation("Search request: q='{Query}', type='{Type}', limit={Limit}, offset={Offset}", 
                q, type, limit, offset);
            
            var searchResults = await _searchService.SearchAsync(q, type, limit, offset, market);
            
            return Ok(searchResults);
        }
        catch (SpotifyApiException ex)
        {
            _logger.LogError(ex, "Spotify API error during search");
            
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse
            {
                Message = "Error communicating with Spotify API",
                ErrorCode = ErrorCodes.SpotifyApiError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (SpotifyRateLimitException ex)
        {
            _logger.LogWarning("Spotify rate limit exceeded: {Message}", ex.Message);
            
            return StatusCode(StatusCodes.Status429TooManyRequests, new ErrorResponse
            {
                Message = "Rate limit exceeded for Spotify API, please try again later",
                ErrorCode = ErrorCodes.RateLimitExceeded,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid search parameters");
            
            return BadRequest(new ErrorResponse
            {
                Message = ex.Message,
                ErrorCode = ErrorCodes.ValidationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search: q='{Query}', type='{Type}'", q, type);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred while performing the search",
                ErrorCode = ErrorCodes.InternalServerError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}