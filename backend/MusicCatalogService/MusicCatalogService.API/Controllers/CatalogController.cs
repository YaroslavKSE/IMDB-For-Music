using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Interfaces;

namespace MusicCatalogService.API.Controllers;

[ApiController]
[Route("api/v1/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ITrackService _trackService;
    private readonly IAlbumService _albumService;
    private readonly ISearchService _searchService;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(
        ITrackService trackService,
        IAlbumService albumService,
        ISearchService searchService,
        ILogger<CatalogController> logger)
    {
        _trackService = trackService;
        _albumService = albumService;
        _searchService = searchService;
        _logger = logger;
    }
    
    [HttpGet("tracks/{spotifyId}")]
    [ProducesResponseType(typeof(TrackDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrack(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Retrieving track with Spotify ID: {SpotifyId}", spotifyId);
            
            var track = await _trackService.GetTrackAsync(spotifyId);
            
            if (track == null)
            {
                return NotFound(new { Message = $"Track with Spotify ID {spotifyId} not found" });
            }
            
            return Ok(track);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving track with Spotify ID: {SpotifyId}", spotifyId);
            return StatusCode(500, new { Message = "An error occurred while retrieving the track" });
        }
    }
    
    [HttpGet("albums/{spotifyId}")]
    [ProducesResponseType(typeof(AlbumDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAlbum(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Retrieving album with Spotify ID: {SpotifyId}", spotifyId);
        
            var album = await _albumService.GetAlbumAsync(spotifyId);
        
            if (album == null)
            {
                return NotFound(new { Message = $"Album with Spotify ID {spotifyId} not found" });
            }
        
            return Ok(album);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving album with Spotify ID: {SpotifyId}", spotifyId);
            return StatusCode(500, new { Message = "An error occurred while retrieving the album" });
        }
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string type, 
        [FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string market = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { Message = "Search query (q) is required" });
            }
            
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new { Message = "Search type is required. Valid types: album, artist, track" });
            }
            
            // Validate type parameter
            var validTypes = new[] { "album", "artist", "track" };
            var requestedTypes = type.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var requestedType in requestedTypes)
            {
                if (!validTypes.Contains(requestedType.Trim().ToLower()))
                {
                    return BadRequest(new { Message = $"Invalid search type: {requestedType}. Valid types: album, artist, track" });
                }
            }
            
            _logger.LogInformation("Search request: q='{Query}', type='{Type}', limit={Limit}, offset={Offset}", 
                q, type, limit, offset);
            
            var searchResults = await _searchService.SearchAsync(q, type, limit, offset, market);
            
            return Ok(searchResults);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid search parameters");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search: q='{Query}', type='{Type}'", q, type);
            return StatusCode(500, new { Message = "An error occurred while performing the search" });
        }
    }
}