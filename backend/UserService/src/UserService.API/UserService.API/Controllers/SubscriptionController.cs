using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Commands;
using UserService.Application.Queries.Subscriptions;
using UserService.Application.Queries.Users;
using UserService.Domain.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/users/subscriptions")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        IMediator mediator,
        ILogger<SubscriptionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Subscribe(SubscribeRequest request)
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

            // Get current user ID from auth ID
            var currentUserQuery = new GetUserProfileQuery(auth0UserId);
            var currentUser = await _mediator.Send(currentUserQuery);

            if (currentUser == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "Current user profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new SubscribeToUserCommand(currentUser.Id, request.UserId);
            var result = await _mediator.Send(command);

            var response = new SubscriptionResponse
            {
                SubscriptionId = result.SubscriptionId,
                FollowerId = result.FollowerId,
                FollowedId = result.FollowedId,
                CreatedAt = result.CreatedAt
            };

            return CreatedAtAction(nameof(Subscribe), response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Code = "NotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (AlreadyExistsException ex)
        {
            return Conflict(new ErrorResponse
            {
                Code = "AlreadySubscribed",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to user");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpDelete("unsubscribe/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe(Guid userId)
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

            // Get current user ID from auth ID
            var currentUserQuery = new GetUserProfileQuery(auth0UserId);
            var currentUser = await _mediator.Send(currentUserQuery);

            if (currentUser == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "Current user profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new UnsubscribeFromUserCommand(currentUser.Id, userId);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new ErrorResponse
                {
                    Code = "SubscriptionNotFound",
                    Message = "Subscription not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from user");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("following")]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

            // Get current user ID from auth ID
            var currentUserQuery = new GetUserProfileQuery(auth0UserId);
            var currentUser = await _mediator.Send(currentUserQuery);

            if (currentUser == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "User profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var query = new GetUserFollowingQuery(currentUser.Id, page, pageSize);
            var result = await _mediator.Send(query);

            var response = new PaginatedSubscriptionsResponse
            {
                Items = result.Items.Select(i => new UserSubscriptionResponse
                {
                    UserId = i.UserId,
                    Username = i.Username,
                    Name = i.Name,
                    Surname = i.Surname,
                    SubscribedAt = i.SubscribedAt
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user following list");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("followers")]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

            // Get current user ID from auth ID
            var currentUserQuery = new GetUserProfileQuery(auth0UserId);
            var currentUser = await _mediator.Send(currentUserQuery);

            if (currentUser == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "User profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var query = new GetUserFollowersQuery(currentUser.Id, page, pageSize);
            var result = await _mediator.Send(query);

            var response = new PaginatedSubscriptionsResponse
            {
                Items = result.Items.Select(i => new UserSubscriptionResponse
                {
                    UserId = i.UserId,
                    Username = i.Username,
                    Name = i.Name,
                    Surname = i.Surname,
                    SubscribedAt = i.SubscribedAt
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when getting users: {Errors}",
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
            _logger.LogError(ex, "Error getting user followers list");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("check/{userId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckSubscriptionStatus(Guid userId)
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

            // Get current user ID from auth ID
            var currentUserQuery = new GetUserProfileQuery(auth0UserId);
            var currentUser = await _mediator.Send(currentUserQuery);

            if (currentUser == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = "Current user profile not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            var query = new CheckSubscriptionStatusQuery(currentUser.Id, userId);
            var isFollowing = await _mediator.Send(query);

            return Ok(isFollowing);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subscription status");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }
}