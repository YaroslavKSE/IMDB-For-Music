using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.Interfaces;

namespace UserService.Application.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IValidator<LogoutCommand> _validator;

    public LogoutCommandHandler(
        IAuth0Service auth0Service,
        ILogger<LogoutCommandHandler> logger,
        IValidator<LogoutCommand> validator)
    {
        _auth0Service = auth0Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Processing logout request");

        // Call Auth0 service to revoke the refresh token
        var result = await _auth0Service.LogoutAsync(request.RefreshToken);

        if (result)
            _logger.LogInformation("User logged out successfully");
        else
            _logger.LogWarning("Logout completed with warnings");

        return result;
    }
}