using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Queries.Preferences;
using UserService.Domain.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/users/preferences")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserPreferencesController> _logger;

    public UserPreferencesController(
        IMediator mediator,
        ILogger<UserPreferencesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPreferences()
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var query = new GetUserPreferencesQuery(auth0UserId);
            var result = await _mediator.Send(query);

            var response = new UserPreferencesResponse
            {
                Artists = result.Artists,
                Albums = result.Albums,
                Tracks = result.Tracks
            };

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User profile not found when accessing preferences: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PreferenceOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPreference([FromBody] AddPreferenceRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new AddUserPreferenceCommand(auth0UserId, request.ItemType, request.SpotifyId);
            var result = await _mediator.Send(command);

            return Ok(new PreferenceOperationResponse
            {
                Success = result,
                Message = $"Successfully added {request.ItemType} preference"
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when adding preference: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found when adding preference: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user preference");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpDelete]
    [ProducesResponseType(typeof(PreferenceOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemovePreference([FromBody] AddPreferenceRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new RemoveUserPreferenceCommand(auth0UserId, request.ItemType, request.SpotifyId);
            var result = await _mediator.Send(command);

            return Ok(new PreferenceOperationResponse
            {
                Success = result,
                Message = $"Successfully removed {request.ItemType} preference"
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when removing preference: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found when removing preference: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user preference");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(PreferenceOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkAddPreferences([FromBody] BulkAddPreferencesRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new BulkAddUserPreferencesCommand(
                auth0UserId,
                request.Artists,
                request.Albums,
                request.Tracks);

            var result = await _mediator.Send(command);

            return Ok(new PreferenceOperationResponse
            {
                Success = result,
                Message = "Successfully added bulk preferences"
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when bulk adding preferences: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found when bulk adding preferences: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk adding user preferences");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpDelete("clear")]
    [ProducesResponseType(typeof(PreferenceOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearPreferences([FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrEmpty(type))
                return BadRequest(new ErrorResponse
                {
                    Code = "ValidationError",
                    Message = "Preference type is required",
                    TraceId = HttpContext.TraceIdentifier
                });

            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new ClearUserPreferencesCommand(auth0UserId, type);
            var result = await _mediator.Send(command);

            return Ok(new PreferenceOperationResponse
            {
                Success = result,
                Message = $"Successfully cleared all {type} preferences"
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when clearing preferences: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found when clearing preferences: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing user preferences");

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}