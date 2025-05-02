using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Queries.Preferences;
using UserService.Application.Queries.Subscriptions;
using UserService.Application.Queries.Users;
using UserService.Domain.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/public/users")]
public class PublicUserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PublicUserController> _logger;

    public PublicUserController(
        IMediator mediator,
        ILogger<PublicUserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string search = null)
    {
        try
        {
            _logger.LogInformation("Getting paginated users: Page {Page}, PageSize {PageSize}, Search: {Search}",
                page, pageSize, search ?? "none");

            var query = new GetUsersQuery(page, pageSize, search);
            var result = await _mediator.Send(query);

            var response = new PaginatedUsersResponse
            {
                Items = result.Items.Select(u => new UserSummaryResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    Surname = u.Surname,
                    AvatarUrl = u.AvatarUrl
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
            _logger.LogError(ex, "Error getting paginated users");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching users",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("id/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublicUserProfileById(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching public profile for user ID: {UserId}", id);

            // Get user by ID
            var userQuery = new GetUserByIdQuery(id);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with ID '{id}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get follower and following counts
            var followerCountQuery = new GetUserFollowersCountQuery(user.Id);
            var followingCountQuery = new GetUserFollowingCountQuery(user.Id);

            var followerCount = await _mediator.Send(followerCountQuery);
            var followingCount = await _mediator.Send(followingCountQuery);

            var response = new PublicUserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                AvatarUrl = user.AvatarUrl
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching public user profile for ID: {UserId}", id);

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

    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublicUserProfile(string username)
    {
        try
        {
            _logger.LogInformation("Fetching public profile for username: {Username}", username);

            // Get user by username
            var userQuery = new GetUserByUsernameQuery(username);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with username '{username}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get follower and following counts
            var followerCountQuery = new GetUserFollowersCountQuery(user.Id);
            var followingCountQuery = new GetUserFollowingCountQuery(user.Id);

            var followerCount = await _mediator.Send(followerCountQuery);
            var followingCount = await _mediator.Send(followingCountQuery);

            var response = new PublicUserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                AvatarUrl = user.AvatarUrl
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching public user profile for username: {Username}", username);

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

    [HttpGet("{username}/followers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserFollowersByUsername(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Fetching followers for username: {Username}, Page: {Page}, PageSize: {PageSize}",
                username, page, pageSize);

            // Get user by username
            var userQuery = new GetUserByUsernameQuery(username);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with username '{username}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get user's followers with pagination
            var query = new GetUserFollowersQuery(user.Id, page, pageSize);
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
            _logger.LogError(ex, "Error fetching followers for username: {Username}", username);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching followers",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("id/{id:guid}/followers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserFollowersById(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Fetching followers for user ID: {UserId}, Page: {Page}, PageSize: {PageSize}",
                id, page, pageSize);

            // Check if user exists
            var userQuery = new GetUserByIdQuery(id);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with ID '{id}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get user's followers with pagination
            var query = new GetUserFollowersQuery(id, page, pageSize);
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
            _logger.LogError(ex, "Error fetching followers for user ID: {UserId}", id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching followers",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("{username}/following")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserFollowingByUsername(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Fetching following for username: {Username}, Page: {Page}, PageSize: {PageSize}",
                username, page, pageSize);

            // Get user by username
            var userQuery = new GetUserByUsernameQuery(username);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with username '{username}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get user's following with pagination
            var query = new GetUserFollowingQuery(user.Id, page, pageSize);
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
            _logger.LogError(ex, "Error fetching following for username: {Username}", username);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching following",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("id/{id:guid}/following")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserFollowingById(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Fetching following for user ID: {UserId}, Page: {Page}, PageSize: {PageSize}",
                id, page, pageSize);

            // Check if user exists
            var userQuery = new GetUserByIdQuery(id);
            var user = await _mediator.Send(userQuery);

            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Code = "UserNotFound",
                    Message = $"User with ID '{id}' not found",
                    TraceId = HttpContext.TraceIdentifier
                });

            // Get user's following with pagination
            var query = new GetUserFollowingQuery(id, page, pageSize);
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
            _logger.LogError(ex, "Error fetching following for user ID: {UserId}", id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching following",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpPost("batch")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BatchUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBatchUserProfiles([FromBody] BatchUserRequest request)
    {
        try
        {
            _logger.LogInformation("Processing batch user profiles request for {Count} users", request.UserIds.Count);

            var query = new GetBatchUsersQuery(request.UserIds);
            var result = await _mediator.Send(query);

            var response = new BatchUserResponse
            {
                Users = result.Users.Select(u => new BatchUserProfileResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    Surname = u.Surname,
                    FollowerCount = u.FollowerCount,
                    FollowingCount = u.FollowingCount,
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when processing batch user profiles: {Errors}",
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
            _logger.LogError(ex, "Error processing batch user profiles");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while fetching user profiles",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpGet("id/{id:guid}/preferences")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPreferencesById(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching preferences for user ID: {UserId}", id);

            var query = new GetUserPreferencesByIdQuery(id);
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
            _logger.LogWarning("User not found when accessing preferences: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences for ID: {UserId}", id);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpGet("{username}/preferences")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPreferencesByUsername(string username)
    {
        try
        {
            _logger.LogInformation("Fetching preferences for username: {Username}", username);

            var query = new GetUserPreferencesByUsernameQuery(username);
            var result = await _mediator.Send(query);

            var response = new UserPreferencesResponse
            {
                Artists = result.Artists,
                Albums = result.Albums,
                Tracks = result.Tracks
            };

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed when accessing preferences: {Errors}",
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
            _logger.LogWarning("User not found when accessing preferences: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences for username: {Username}", username);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An unexpected error occurred",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}