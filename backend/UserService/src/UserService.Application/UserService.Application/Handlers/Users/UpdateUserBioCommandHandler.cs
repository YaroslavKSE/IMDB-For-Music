using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class UpdateUserBioCommandHandler : IRequestHandler<UpdateUserBioCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserBioCommandHandler> _logger;
    private readonly IValidator<UpdateUserBioCommand> _validator;

    public UpdateUserBioCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserBioCommandHandler> logger,
        IValidator<UpdateUserBioCommand> validator)
    {
        _userRepository = userRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserResponse> Handle(UpdateUserBioCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        // Get the user by Auth0 ID
        var user = await _userRepository.GetByAuth0IdAsync(command.Auth0UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for Auth0 ID: {Auth0UserId}", command.Auth0UserId);
            throw new NotFoundException($"User with Auth0 ID {command.Auth0UserId} not found");
        }

        // Update the user's bio
        user.UpdateBio(command.Bio);

        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Updated bio for user ID: {UserId}, Auth0 ID: {Auth0UserId}",
            user.Id, command.Auth0UserId);

        // Return the updated user profile
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Name = user.Name,
            Surname = user.Surname,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}