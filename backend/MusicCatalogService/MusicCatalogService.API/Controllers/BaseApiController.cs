using Microsoft.AspNetCore.Mvc;
using MusicCatalogService.Core.Exceptions;
using MusicCatalogService.Core.Responses;
using ErrorResponse = MusicCatalogService.API.Models.ErrorResponse;

namespace MusicCatalogService.API.Controllers;

/// <summary>
/// Base controller class that provides common functionality and error handling for all API controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger<BaseApiController> _logger;

    protected BaseApiController(ILogger<BaseApiController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Execute an API action with standardized error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the action</typeparam>
    /// <param name="action">The function to execute</param>
    /// <param name="errorMessage">Message to log if an unexpected error occurs</param>
    /// <param name="resourceId">Optional resource ID for logging purposes</param>
    /// <returns>IActionResult with appropriate status code and response</returns>
    protected async Task<IActionResult> ExecuteApiActionAsync<T>(
        Func<Task<T>> action, 
        string errorMessage,
        string resourceId = null) where T : class
    {
        try
        {
            var result = await action();
            
            if (result == null)
            {
                return NotFound(new ErrorResponse
                { 
                    Message = $"Resource not found",
                    ErrorCode = ErrorCodes.ResourceNotFound,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            
            return Ok(result);
        }
        catch (SpotifyResourceNotFoundException ex)
        {
            _logger.LogWarning("Resource not found in Spotify: {ResourceId}, Message: {Message}", 
                ex.ResourceId, ex.Message);
            
            return NotFound(new ErrorResponse
            {
                Message = ex.Message,
                ErrorCode = ErrorCodes.ResourceNotFound,
                TraceId = HttpContext.TraceIdentifier,
                Details = new SpotifyErrorDetails 
                { 
                    SpotifyId = resourceId ?? ex.ResourceId,
                    StatusCode = 404
                }
            });
        }
        catch (SpotifyAuthorizationException ex)
        {
            _logger.LogWarning("Spotify authorization error: {Message}", ex.Message);
            
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
            {
                Message = "Authorization error occurred when accessing Spotify API",
                ErrorCode = ErrorCodes.AuthorizationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (SpotifyRateLimitException ex)
        {
            _logger.LogWarning("Spotify rate limit exceeded: {Message}", ex.Message);
            
            return StatusCode(StatusCodes.Status429TooManyRequests, new ErrorResponse
            {
                Message = "Rate limit exceeded for Spotify API, please try again later",
                ErrorCode = ErrorCodes.RateLimitExceeded,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (SpotifyApiException ex)
        {
            _logger.LogError(ex, "Spotify API error when accessing resource: {ResourceId}", resourceId ?? "unknown");
            
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse
            {
                Message = "Error communicating with Spotify API",
                ErrorCode = ErrorCodes.SpotifyApiError,
                TraceId = HttpContext.TraceIdentifier,
                Details = new SpotifyErrorDetails 
                { 
                    SpotifyId = resourceId,
                    StatusCode = (int)ex.StatusCode
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred",
                ErrorCode = ErrorCodes.InternalServerError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
    
    /// <summary>
    /// Execute an API creation action with standardized error handling for POST requests.
    /// </summary>
    /// <typeparam name="T">The return type of the action</typeparam>
    /// <param name="action">The function to execute</param>
    /// <param name="errorMessage">Message to log if an unexpected error occurs</param>
    /// <param name="resourceId">Optional resource ID for logging purposes</param>
    /// <param name="createSuccessResponse">Function to create success response from result</param>
    /// <param name="createResourceUri">Function to create resource URI for Location header</param>
    /// <returns>IActionResult with appropriate status code and response</returns>
    protected async Task<IActionResult> ExecuteCreateActionAsync<T>(
        Func<Task<T>> action, 
        string errorMessage,
        string resourceId,
        Func<T, object> createSuccessResponse,
        Func<T, object> createResourceUri) where T : class
    {
        try
        {
            var result = await action();
            
            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = $"Resource with ID {resourceId} not found",
                    ErrorCode = ErrorCodes.ResourceNotFound,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            
            // Create success response
            var response = createSuccessResponse(result);
        
            // Get the controller name from the current controller context
            var controllerName = ControllerContext.ActionDescriptor.ControllerName.ToLowerInvariant();
        
            // Create URL directly
            var resourceUri = $"/api/v1/catalog/{controllerName}/{((dynamic)createResourceUri(result)).catalogId}";
        
            // Use Created instead of CreatedAtAction
            return Created(resourceUri, response);
        }
        catch (SpotifyResourceNotFoundException ex)
        {
            _logger.LogWarning("Cannot save - resource not found in Spotify: {ResourceId}", resourceId);
            
            return NotFound(new ErrorResponse
            {
                Message = $"Resource with ID {resourceId} not found",
                ErrorCode = ErrorCodes.ResourceNotFound,
                TraceId = HttpContext.TraceIdentifier,
                Details = new SpotifyErrorDetails 
                { 
                    SpotifyId = resourceId,
                    StatusCode = 404
                }
            });
        }
        catch (SpotifyAuthorizationException ex)
        {
            _logger.LogWarning("Spotify authorization error: {Message}", ex.Message);
            
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
            {
                Message = "Authorization error occurred when accessing Spotify API",
                ErrorCode = ErrorCodes.AuthorizationError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (SpotifyRateLimitException ex)
        {
            _logger.LogWarning("Spotify rate limit exceeded: {Message}", ex.Message);
            
            return StatusCode(StatusCodes.Status429TooManyRequests, new ErrorResponse
            {
                Message = "Rate limit exceeded for Spotify API, please try again later",
                ErrorCode = ErrorCodes.RateLimitExceeded,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (SpotifyApiException ex)
        {
            _logger.LogError(ex, "Spotify API error when saving resource: {ResourceId}", resourceId);
            
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse
            {
                Message = "Error communicating with Spotify API",
                ErrorCode = ErrorCodes.SpotifyApiError,
                TraceId = HttpContext.TraceIdentifier,
                Details = new SpotifyErrorDetails 
                { 
                    SpotifyId = resourceId,
                    StatusCode = (int?)ex.StatusCode ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred",
                ErrorCode = ErrorCodes.InternalServerError,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
    
    /// <summary>
    /// This method needs to be implemented by all derived controllers to support
    /// the CreatedAtAction result in ExecuteCreateActionAsync.
    /// </summary>
    [NonAction]
    public abstract Task<IActionResult> GetById(Guid id);
}