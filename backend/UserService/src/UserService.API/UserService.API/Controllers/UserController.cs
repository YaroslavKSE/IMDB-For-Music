using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Responses;
using UserService.Application.Queries;

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
                {
                    _logger.LogInformation("Available claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
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
            {
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "User profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var response = new UserProfileResponse
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                Name = userProfile.Name,
                Surname = userProfile.Surname
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
}