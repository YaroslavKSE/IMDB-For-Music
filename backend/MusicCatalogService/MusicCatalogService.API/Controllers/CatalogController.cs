using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Services;

namespace MusicCatalogService.API.Controllers;

[ApiController]
[Route("api/v1/catalog")]
public class CatalogController : ControllerBase
{
    private readonly IMusicCatalogService _musicCatalogService;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(
        IMusicCatalogService musicCatalogService,
        ILogger<CatalogController> logger)
    {
        _musicCatalogService = musicCatalogService;
        _logger = logger;
    }

    [HttpGet("albums/{spotifyId}")]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAlbum(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Getting album with Spotify ID: {SpotifyId}", spotifyId);
            var album = await _musicCatalogService.GetAlbumAsync(spotifyId);
            return Ok(album);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving album {SpotifyId}", spotifyId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the album." });
        }
    }

    [HttpGet("tracks/{spotifyId}")]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrack(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Getting track with Spotify ID: {SpotifyId}", spotifyId);
            var track = await _musicCatalogService.GetTrackAsync(spotifyId);
            return Ok(track);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving track {SpotifyId}", spotifyId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the track." });
        }
    }

    [HttpGet("artists/{spotifyId}")]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetArtist(string spotifyId)
    {
        try
        {
            _logger.LogInformation("Getting artist with Spotify ID: {SpotifyId}", spotifyId);
            var artist = await _musicCatalogService.GetArtistAsync(spotifyId);
            return Ok(artist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artist {SpotifyId}", spotifyId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the artist." });
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string query, 
        [FromQuery] string type = "album,track,artist",
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Query parameter cannot be empty." });
        }

        try
        {
            _logger.LogInformation("Searching for {Query} with type {Type}", query, type);
            var searchResults = await _musicCatalogService.SearchAsync(query, type, limit, offset);
            return Ok(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for {Query}", query);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while searching." });
        }
    }

    [HttpGet("new-releases")]
    [ProducesResponseType(typeof(NewReleasesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNewReleases(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            _logger.LogInformation("Getting new releases, limit: {Limit}, offset: {Offset}", limit, offset);
            var newReleases = await _musicCatalogService.GetNewReleasesAsync(limit, offset);
            return Ok(newReleases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving new releases");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving new releases." });
        }
    }
}