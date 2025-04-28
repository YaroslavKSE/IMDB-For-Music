using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Auth;

public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, LoginResponseDto>
{
    private readonly IAuth0Service _auth0Service;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SocialLoginCommandHandler> _logger;
    private readonly IValidator<SocialLoginCommand> _validator;

    public SocialLoginCommandHandler(
        IAuth0Service auth0Service,
        IUserRepository userRepository,
        ILogger<SocialLoginCommandHandler> logger,
        IValidator<SocialLoginCommand> validator)
    {
        _auth0Service = auth0Service;
        _userRepository = userRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<LoginResponseDto> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        // Verify token with Auth0 and get user info
        var userInfo = await _auth0Service.GetUserInfoAsync(request.AccessToken);

        // Check if the user already exists in our database
        var existingUser = await _userRepository.GetByAuth0IdAsync(userInfo.UserId);

        if (existingUser == null)
        {
            // Extract name and surname from user info
            var nameParts = userInfo.Name?.Split(' ') ?? Array.Empty<string>();
            var name = nameParts.Length > 0 ? nameParts[0] : "User";
            var surname = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            // Use the nickname provided by Auth0
            var username = userInfo.Username;

            // Ensure username is unique
            var attempt = 1;
            var candidateUsername = username;
            while (await _userRepository.GetByUsernameAsync(candidateUsername) != null)
                candidateUsername = $"{username}{attempt++}";
            username = candidateUsername;

            // Create new user
            var newUser = User.Create(
                userInfo.Email,
                username,
                name,
                surname,
                userInfo.UserId);

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            await _auth0Service.AssignDefaultRoleAsync(userInfo.UserId);

            _logger.LogInformation(
                "New user created from social login: {Provider}, Email: {Email}, Username: {Username}",
                request.Provider, userInfo.Email, username);
        }
        else
        {
            _logger.LogInformation("Existing user logged in via social login: {Provider}, Email: {Email}",
                request.Provider, userInfo.Email);
        }

        // Get Auth0 tokens
        var authTokenResponse = await _auth0Service.GetTokensForSocialUserAsync(request.AccessToken);

        return new LoginResponseDto
        {
            AccessToken = authTokenResponse.AccessToken,
            RefreshToken = authTokenResponse.RefreshToken,
            ExpiresIn = authTokenResponse.ExpiresIn,
            TokenType = authTokenResponse.TokenType
        };
    }
}