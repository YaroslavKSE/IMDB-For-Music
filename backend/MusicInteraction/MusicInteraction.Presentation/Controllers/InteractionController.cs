using System.Runtime.InteropServices.JavaScript;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicInteraction.Application;

namespace MusicInteraction.Presentation.Controllers;

[ApiController]
public class InteractionController: ControllerBase
{
    private readonly IMediator mediator;

    public InteractionController(IMediator _mediator)
    {
        this.mediator = _mediator;
    }

    [HttpPost("postInteraction")]
    public async Task<IActionResult> PostInteraction([FromBody]PostInteractionRequest request)
    {
        PostInteractionCommand command = new PostInteractionCommand()
        {
            UserId = request.UserId,
            ItemId = request.ItemId,
            ItemType = request.ItemType,
            IsLiked = request.IsLiked,
            ReviewText = request.ReviewText,

            // Handle grading options
            UseComplexGrading = request.UseComplexGrading,
            BasicGrade = request.BasicGrade,
            GradingMethodId = request.GradingMethodId,
            GradeInputs = request.GradeInputs
        };

        var result = await mediator.Send(command);

        if (!result.InteractionCreated)
        {
            return BadRequest("Error interaction not created");
        }

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            // Return the result with a 200 status code, but include the error message
            // This way the client can see what went wrong while still getting the interaction ID
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

}