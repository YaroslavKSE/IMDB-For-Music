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

    [HttpPost("writeReview")]
    public async Task<IActionResult> WriteReview([FromBody]WriteReviewRequest request)
    {
        WriteReviewCommand command = new WriteReviewCommand()
            {UserId = request.UserId, ItemId = request.ItemId, ReviewText = request.ReviewText};
        var result = await mediator.Send(command);
        if(!result.ReviewCreated) return BadRequest("Error review not created");
        return Ok("Success");
    }

    [HttpPost("postInteraction")]
    public async Task<IActionResult> PostInteraction([FromBody]PostInteractionRequest request)
    {
        PostInteractionCommand command = new PostInteractionCommand()
        {UserId = request.UserId, ItemId = request.ItemId, ItemType = request.ItemType,
            IsLiked = request.IsLiked, ReviewText = request.ReviewText, Grade = request.Grade};
        var result = await mediator.Send(command);
        if(!result.InteractionCreated) return BadRequest("Error interaction not created");
        return Ok(result);
    }

    [HttpGet("getInteractions")]
    public async Task<IActionResult> GetInteractions()
    {
        var result = await mediator.Send(new GetInteractionsCommand());
        if (result.InteractionsEmpty) return NotFound("There are no interactions");
        return Ok(result.Interactions);
    }

}