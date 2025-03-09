using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using UserService.API.Models.Requests;
using UserService.API.Models.Responses;
using UserService.Application.Commands;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Models.Auth0.Exceptions;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterSuccessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        try
        {
            _logger.LogInformation("Registering new user with email: {Email}", request.Email);

            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.Name,
                request.Surname);

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User successfully registered: {Id}", result.UserId);
            
            var response = new RegisterSuccessResponse
            {
                UserId = result.UserId,
                Message = "User created successfully"
            };

            return CreatedAtAction(
                nameof(Register), 
                new { id = result.UserId }, 
                response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed during registration: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (UserAlreadyExistsException ex)
        {
            _logger.LogWarning("Registration failed - user already exists: {Message}", ex.Message);
            
            return Conflict(new ErrorResponse
            {
                Code = "UserAlreadyExists",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Auth0Exception ex)
        {
            _logger.LogError(ex, "Auth0 error during registration");
            
            return BadRequest(new ErrorResponse
            {
                Code = ex.Error.Code ?? "Auth0Error",
                Message = ex.Error.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred during registration",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }    
    
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var command = new LoginCommand(
                request.Email,
                request.Password);

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User successfully logged in: {Email}", request.Email);
            
            var response = new LoginResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                IdToken = result.IdToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType
            };
            
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed during login: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            
            return BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Auth0Exception ex)
        {
            _logger.LogWarning("Auth0 error during login: {Error}", ex.Error.Message);
            
            return Unauthorized(new ErrorResponse
            {
                Code = "AuthenticationError",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred during login",
                    TraceId = HttpContext.TraceIdentifier
                });
        }
    }
}