using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;
    private readonly IValidator<UpdateUserProfileCommand> _validator;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserProfileCommandHandler> logger,
        IValidator<UpdateUserProfileCommand> validator)
    {
        _userRepository = userRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserResponse> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        // Get the user by Auth0 ID
        var user = await _userRepository.GetByAuth0IdAsync(command.Auth0UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for Auth0 ID: {Auth0UserId}", command.Auth0UserId);
            return null;
        }

        // Check if the username is being changed and if the new username is taken by another user
        if (!string.IsNullOrEmpty(command.Username) && command.Username != user.Username)
        {
            var existingUserWithUsername = await _userRepository.GetByUsernameAsync(command.Username);
            if (existingUserWithUsername != null && existingUserWithUsername.Id != user.Id)
                throw new UsernameAlreadyTakenException(command.Username);
        }

        // Determine which fields to update
        var usernameToUpdate = !string.IsNullOrEmpty(command.Username) ? command.Username : user.Username;
        var nameToUpdate = !string.IsNullOrEmpty(command.Name) ? command.Name : user.Name;
        var surnameToUpdate = !string.IsNullOrEmpty(command.Surname) ? command.Surname : user.Surname;

        // Update the user
        user.Update(usernameToUpdate, nameToUpdate, surnameToUpdate);

        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Updated profile for user ID: {UserId}, Auth0 ID: {Auth0UserId}",
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
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}