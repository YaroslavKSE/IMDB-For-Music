using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.AvatarsHandlers;

public class UpdateUserAvatarCommandHandler : IRequestHandler<UpdateUserAvatarCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IS3StorageService _s3Service;
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<UpdateUserAvatarCommandHandler> _logger;
    private readonly IValidator<UpdateUserAvatarCommand> _validator;

    public UpdateUserAvatarCommandHandler(
        IUserRepository userRepository,
        IS3StorageService s3Service,
        IAuth0Service auth0Service,
        ILogger<UpdateUserAvatarCommandHandler> logger,
        IValidator<UpdateUserAvatarCommand> validator)
    {
        _userRepository = userRepository;
        _s3Service = s3Service;
        _auth0Service = auth0Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserResponse> Handle(UpdateUserAvatarCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get the user
        var user = await _userRepository.GetByAuth0IdAsync(command.Auth0UserId);
        if (user == null)
            throw new NotFoundException($"User with Auth0 ID '{command.Auth0UserId}' not found");

        // If user already has an avatar, delete it first
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            await _s3Service.DeleteUserAvatarAsync(user.Id);
        }

        // Upload the new avatar
        var avatarUrl = await _s3Service.UploadUserAvatarAsync(command.File, user.Id);

        // Update the user entity
        user.UpdateAvatar(avatarUrl);
        await _userRepository.SaveChangesAsync();

        // Update the user's avatar in Auth0
        await _auth0Service.UpdateUserPictureAsync(command.Auth0UserId, avatarUrl);

        _logger.LogInformation("Updated avatar for user {UserId} with Auth0 ID {Auth0UserId}", user.Id, command.Auth0UserId);

        // Return updated user
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Name = user.Name,
            Surname = user.Surname,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}