using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.AvatarsHandlers;

public class GetAvatarUploadUrlCommandHandler : IRequestHandler<GetAvatarUploadUrlCommand, PresignedUploadResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IS3StorageService _s3Service;
    private readonly ILogger<GetAvatarUploadUrlCommandHandler> _logger;
    private readonly IValidator<GetAvatarUploadUrlCommand> _validator;

    public GetAvatarUploadUrlCommandHandler(
        IUserRepository userRepository,
        IS3StorageService s3Service,
        ILogger<GetAvatarUploadUrlCommandHandler> logger,
        IValidator<GetAvatarUploadUrlCommand> validator)
    {
        _userRepository = userRepository;
        _s3Service = s3Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<PresignedUploadResponse> Handle(GetAvatarUploadUrlCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get the user
        var user = await _userRepository.GetByAuth0IdAsync(command.Auth0UserId);
        if (user == null)
            throw new NotFoundException($"User with Auth0 ID '{command.Auth0UserId}' not found");

        // Generate presigned URL
        var response = await _s3Service.GeneratePresignedUploadUrlAsync(user.Id, command.ContentType);

        _logger.LogInformation("Generated presigned upload URL for user {UserId}", user.Id);

        return response;
    }
}

public class CompleteAvatarUploadCommandHandler : IRequestHandler<CompleteAvatarUploadCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<CompleteAvatarUploadCommandHandler> _logger;
    private readonly IValidator<CompleteAvatarUploadCommand> _validator;

    public CompleteAvatarUploadCommandHandler(
        IUserRepository userRepository,
        IAuth0Service auth0Service,
        ILogger<CompleteAvatarUploadCommandHandler> logger,
        IValidator<CompleteAvatarUploadCommand> validator)
    {
        _userRepository = userRepository;
        _auth0Service = auth0Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserResponse> Handle(CompleteAvatarUploadCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get the user
        var user = await _userRepository.GetByAuth0IdAsync(command.Auth0UserId);
        if (user == null)
            throw new NotFoundException($"User with Auth0 ID '{command.Auth0UserId}' not found");

        // Update the user entity with the new avatar URL
        user.UpdateAvatar(command.AvatarUrl);
        await _userRepository.SaveChangesAsync();

        // Update the user's avatar in Auth0
        await _auth0Service.UpdateUserPictureAsync(command.Auth0UserId, command.AvatarUrl);

        _logger.LogInformation("Completed avatar upload for user {UserId} with Auth0 ID {Auth0UserId}", user.Id,
            command.Auth0UserId);

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