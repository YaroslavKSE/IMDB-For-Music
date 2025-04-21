// MusicListsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicLists.Application.Commands;
using MusicLists.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MusicLists.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicListsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MusicListsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateMusicListCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPut("{listId:guid}")]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] UpdateMusicListCommand command)
    {
        if (listId != command.ListId)
        {
            return BadRequest("List ID in URL doesn't match the one in request body");
        }

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> DeleteList(Guid listId)
    {
        var command = new DeleteMusicListCommand { ListId = listId };
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("{listId:guid}/like")]
    public async Task<IActionResult> AddListLike(Guid listId, [FromBody] string userId)
    {
        var command = new AddListLikeCommand
        {
            ListId = listId,
            UserId = userId
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("{listId:guid}/like")]
    public async Task<IActionResult> RemoveListLike(Guid listId, [FromQuery] string userId)
    {
        var command = new RemoveListLikeCommand
        {
            ListId = listId,
            UserId = userId
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("{listId:guid}/like/{userId}")]
    public async Task<IActionResult> CheckUserLikedList(Guid listId, string userId)
    {
        var command = new CheckUserLikedListCommand
        {
            ListId = listId,
            UserId = userId
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}