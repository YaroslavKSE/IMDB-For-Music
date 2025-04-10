using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Models.Responses;
using UserService.Application.Queries;

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
                    Surname = u.Surname
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
}