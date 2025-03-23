using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicInteraction.Application;

namespace MusicInteraction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InteractionController : ControllerBase
{
    private readonly IMediator mediator;

    public InteractionController(IMediator _mediator)
    {
        this.mediator = _mediator;
    }

    [HttpPost("postInteraction")]
    public async Task<IActionResult> PostInteraction([FromBody] PostInteractionCommand command)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(command.UserId) ||
            string.IsNullOrEmpty(command.ItemId) ||
            string.IsNullOrEmpty(command.ItemType))
        {
            return BadRequest("UserId, ItemId, and ItemType are required fields");
        }

        // Validate that if UseComplexGrading is true, GradingMethodId is provided
        if (command.UseComplexGrading && (!command.GradingMethodId.HasValue || command.GradingMethodId == Guid.Empty))
        {
            return BadRequest("GradingMethodId is required when UseComplexGrading is true");
        }

        var result = await mediator.Send(command);

        if (!result.InteractionCreated)
        {
            return BadRequest("Error interaction not created");
        }

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            return Ok(result);
        }

        return Ok(result);
    }

    [HttpGet("getInteractions")]
    public async Task<IActionResult> GetInteractions()
    {
        var result = await mediator.Send(new GetInteractionsCommand());
        if (result.InteractionsEmpty) return NotFound("There are no interactions");
        return Ok(result.Interactions);
    }

    [HttpGet("getInteraction/{id}")]
    public async Task<IActionResult> GetInteractionById(Guid id)
    {
        var command = new GetInteractionByIdCommand() { InteractionId = id };
        var result = await mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Interaction);
    }

    [HttpGet("getLikes")]
    public async Task<IActionResult> GetLikes()
    {
        var result = await mediator.Send(new GetLikesCommand());
        if (result.LikesEmpty) return NotFound("There are no likes");
        return Ok(result.Likes);
    }

    [HttpGet("getReviews")]
    public async Task<IActionResult> GetReviews()
    {
        var result = await mediator.Send(new GetReviewsCommand());
        if (result.ReviewsEmpty) return NotFound("There are no reviews");
        return Ok(result.Reviews);
    }

    [HttpGet("getRatings")]
    public async Task<IActionResult> GetRatings()
    {
        var result = await mediator.Send(new GetRatingsCommand());
        if (result.RatingsEmpty) return NotFound("There are no ratings");
        return Ok(result.Ratings);
    }

    [HttpGet("getRating/{id}")]
    public async Task<IActionResult> GetRatingById(Guid id)
    {
        var command = new GetRatingByIdCommand { RatingId = id };
        var result = await mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Rating);
    }
}