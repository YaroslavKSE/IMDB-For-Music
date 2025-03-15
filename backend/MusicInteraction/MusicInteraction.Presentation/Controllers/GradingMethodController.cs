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

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicGradingMethods()
    {
        var command = new GetPublicGradingMethodsCommand();
        var result = await mediator.Send(command);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.GradingMethods);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGradingMethodById(Guid id)
    {
        var command = new GetGradingMethodByIdCommand
        {
            GradingMethodId = id
        };

        var result = await mediator.Send(command);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.GradingMethod);
    }
}