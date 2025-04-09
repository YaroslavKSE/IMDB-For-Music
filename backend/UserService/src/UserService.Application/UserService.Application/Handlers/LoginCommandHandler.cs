using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IValidator<LoginCommand> _validator;

    public LoginCommandHandler(
        IAuth0Service auth0Service,
        ILogger<LoginCommandHandler> logger,
        IValidator<LoginCommand> validator)
    {
        _auth0Service = auth0Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        // Attempt to login via Auth0
        var authTokenResponse = await _auth0Service.LoginAsync(request.Email, request.Password);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        // Map the auth token response to our response DTO
        return new LoginResponseDto
        {
            AccessToken = authTokenResponse.AccessToken,
            RefreshToken = authTokenResponse.RefreshToken,
            ExpiresIn = authTokenResponse.ExpiresIn,
            TokenType = authTokenResponse.TokenType
        };
    }
}