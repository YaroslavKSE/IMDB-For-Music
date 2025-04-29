using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Domain.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/users/avatars")]
[Authorize]
public class AvatarController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AvatarController> _logger;

    public AvatarController(
        IMediator mediator,
        ILogger<AvatarController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new UpdateUserAvatarCommand(auth0UserId, request.File);
            var result = await _mediator.Send(command);

            var response = new UserProfileResponse
            {
                Id = result.Id,
                Email = result.Email,
                Username = result.Username,
                Name = result.Name,
                Surname = result.Surname,
                AvatarUrl = result.AvatarUrl
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
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Code = "NotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while uploading avatar",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpDelete]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAvatar()
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new DeleteUserAvatarCommand(auth0UserId);
            var result = await _mediator.Send(command);

            var response = new UserProfileResponse
            {
                Id = result.Id,
                Email = result.Email,
                Username = result.Username,
                Name = result.Name,
                Surname = result.Surname,
                AvatarUrl = result.AvatarUrl
            };

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Code = "NotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatar");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while deleting avatar",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpPost("presigned-url")]
    [ProducesResponseType(typeof(PresignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPresignedUrl([FromBody] GetPresignedUrlRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new GetAvatarUploadUrlCommand(auth0UserId, request.ContentType);
            var result = await _mediator.Send(command);

            var response = new PresignedUrlResponse
            {
                Url = result.Url,
                FormData = result.FormData,
                ObjectKey = result.ObjectKey,
                AvatarUrl = result.AvatarUrl,
                ExpiresInSeconds = result.ExpiresInSeconds
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
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Code = "NotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while generating presigned URL",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }

    [HttpPost("complete-upload")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteUpload([FromBody] CompleteAvatarUploadRequest request)
    {
        try
        {
            var auth0UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
                return Unauthorized(new ErrorResponse
                {
                    Code = "InvalidToken",
                    Message = "User identifier not found in token",
                    TraceId = HttpContext.TraceIdentifier
                });

            var command = new CompleteAvatarUploadCommand(auth0UserId, request.ObjectKey, request.AvatarUrl);
            var result = await _mediator.Send(command);

            var response = new UserProfileResponse
            {
                Id = result.Id,
                Email = result.Email,
                Username = result.Username,
                Name = result.Name,
                Surname = result.Surname,
                AvatarUrl = result.AvatarUrl
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
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Code = "NotFound",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing avatar upload");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while completing avatar upload",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }
}