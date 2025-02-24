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

    [HttpGet("getInteractions")]
    public async Task<IActionResult> GetInteractions()
    {
        var result = await mediator.Send(new GetInteractionsCommand());
        if (result.InteractionsEmpty) return NotFound("There are no interactions");
        return Ok(result.Interactions);
    }

}