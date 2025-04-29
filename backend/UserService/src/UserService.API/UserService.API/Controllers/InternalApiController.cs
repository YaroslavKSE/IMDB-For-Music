using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Responses;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/internal")]
// [Authorize(Policy = "InternalApiPolicy")] // This would require a specific policy for internal services
public class InternalApiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InternalApiController> _logger;

    public InternalApiController(
        IMediator mediator,
        ILogger<InternalApiController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("followers")]
    [ProducesResponseType(typeof(FollowerIdsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFollowerIds(
        [FromQuery] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 2000)
    {
        try
        {
            _logger.LogInformation("Getting follower IDs for user {UserId}, page {Page}, pageSize {PageSize}",
                userId, page, pageSize);

            var query = new GetUserFollowerIdsQuery(userId, page, pageSize);
            var result = await _mediator.Send(query);

            var response = new FollowerIdsResponse
            {
                FollowerIds = result.FollowerIds,
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
            _logger.LogWarning("Validation error when getting follower IDs: {Error}",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);

            return NotFound(new ErrorResponse
            {
                Code = "UserNotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting follower IDs for user {UserId}", userId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while retrieving follower IDs",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }
}