using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Commands;
using UserService.Application.Queries;
using UserService.Domain.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IMediator mediator,
        ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        try
        {
            // Extract the user ID from the claims (set by the Auth0 JWT)
            // Look for both common claim types for the subject identifier
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                              ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(auth0UserId))
            {
                _logger.LogWarning("Auth0 user ID not found in token claims");

                // Log all available claims for debugging
                foreach (var claim in User.Claims)
                    _logger.LogInformation("Available claim: {Type} = {Value}", claim.Type, claim.Value);

                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            _logger.LogInformation("Fetching profile for Auth0 user: {Auth0UserId}", auth0UserId);

            var query = new GetUserProfileQuery(auth0UserId);
            var userProfile = await _mediator.Send(query);

            if (userProfile == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "User profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var response = new UserProfileResponse
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                Name = userProfile.Name,
                Username = userProfile.Username,
                Surname = userProfile.Surname,
                AvatarUrl = userProfile.AvatarUrl
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching user profile",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserProfile(UpdateUserProfileRequest request)
    {
        try
        {
            // Extract the Auth0 user ID from claims
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                              ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(auth0UserId))
            {
                _logger.LogWarning("Auth0 user ID not found in token claims during profile update");

                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            _logger.LogInformation("Updating profile for Auth0 user: {Auth0UserId}", auth0UserId);

            var command = new UpdateUserProfileCommand(
                auth0UserId,
                request.Username,
                request.Name,
                request.Surname);

            var updatedProfile = await _mediator.Send(command);

            if (updatedProfile == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "User profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var response = new UserProfileResponse
            {
                Id = updatedProfile.Id,
                Email = updatedProfile.Email,
                Name = updatedProfile.Name,
                Surname = updatedProfile.Surname,
                Username = updatedProfile.Username,
                AvatarUrl = updatedProfile.AvatarUrl
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed during profile update: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (UsernameAlreadyTakenException ex)
        {
            _logger.LogWarning("Profile update failed - username already taken: {Message}", ex.Message);

            return Conflict(new ErrorResponse
            {
                Code = "UsernameAlreadyTaken",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while updating profile",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching user by ID: {UserId}", id);

            var query = new GetUserByIdQuery(id);
            var userProfile = await _mediator.Send(query);

            if (userProfile == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var response = new UserProfileResponse
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                Name = userProfile.Name,
                Username = userProfile.Username,
                Surname = userProfile.Surname,
                AvatarUrl = userProfile.AvatarUrl
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when fetching user by ID: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by ID");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching user profile",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        try
        {
            _logger.LogInformation("Fetching user by username: {Username}", username);

            var query = new GetUserByUsernameQuery(username);
            var userProfile = await _mediator.Send(query);

            if (userProfile == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with username '{username}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var response = new UserProfileResponse
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                Name = userProfile.Name,
                Username = userProfile.Username,
                Surname = userProfile.Surname,
                AvatarUrl = userProfile.AvatarUrl
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when fetching user by username: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by username");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching user profile",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            _logger.LogInformation("Fetching user by email: {Email}", email);

            var query = new GetUserByEmailQuery(email);
            var userProfile = await _mediator.Send(query);

            if (userProfile == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with email '{email}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var response = new UserProfileResponse
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                Name = userProfile.Name,
                Username = userProfile.Username,
                Surname = userProfile.Surname,
                AvatarUrl = userProfile.AvatarUrl
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when fetching user by email: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by email");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching user profile",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }
}