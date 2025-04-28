using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Avatars;

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

    public async Task<PresignedUploadResponse> Handle(GetAvatarUploadUrlCommand command,
        CancellationToken cancellationToken)
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