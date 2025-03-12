using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicInteraction.Application;

namespace MusicInteraction.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradingMethodController : ControllerBase
{
    private readonly IMediator mediator;

    public GradingMethodController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGradingMethod([FromBody] CreateGradingMethodCommand request)
    {
        var result = await mediator.Send(request);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result);
    }
}