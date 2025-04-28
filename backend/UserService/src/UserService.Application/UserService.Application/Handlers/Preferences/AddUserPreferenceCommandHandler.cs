using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Queries.Preferences;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Preferences;

public class AddUserPreferenceCommandHandler : IRequestHandler<AddUserPreferenceCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPreferenceRepository _preferenceRepository;
    private readonly ILogger<AddUserPreferenceCommandHandler> _logger;
    private readonly IValidator<AddUserPreferenceCommand> _validator;

    public AddUserPreferenceCommandHandler(
        IUserRepository userRepository,
        IUserPreferenceRepository preferenceRepository,
        ILogger<AddUserPreferenceCommandHandler> logger,
        IValidator<AddUserPreferenceCommand> validator)
    {
        _userRepository = userRepository;
        _preferenceRepository = preferenceRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<bool> Handle(AddUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get user by Auth0 ID
        var user = await _userRepository.GetByAuth0IdAsync(request.Auth0UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found with Auth0 ID: {Auth0UserId}", request.Auth0UserId);
            throw new NotFoundException($"User with Auth0 ID {request.Auth0UserId} not found");
        }

        // Parse the item type
        if (!TryParseItemType(request.ItemType, out var itemType))
            throw new ValidationException($"Invalid item type: {request.ItemType}");

        // Check if preference already exists
        var exists = await _preferenceRepository.UserPreferenceExistsAsync(user.Id, itemType, request.SpotifyId);
        if (exists)
        {
            _logger.LogInformation(
                "Preference already exists for user {UserId}, type {ItemType}, Spotify ID {SpotifyId}",
                user.Id, itemType, request.SpotifyId);
            return true; // Already exists, consider it a success
        }

        // Create new preference
        var preference = UserPreference.Create(user.Id, itemType, request.SpotifyId);

        // Add preference
        await _preferenceRepository.AddAsync(preference);
        await _preferenceRepository.SaveChangesAsync();

        _logger.LogInformation("Added preference for user {UserId}, type {ItemType}, Spotify ID {SpotifyId}",
            user.Id, itemType, request.SpotifyId);

        return true;
    }

    private bool TryParseItemType(string itemTypeString, out PreferenceType itemType)
    {
        switch (itemTypeString.ToLower())
        {
            case "artist":
                itemType = PreferenceType.Artist;
                return true;
            case "album":
                itemType = PreferenceType.Album;
                return true;
            case "track":
                itemType = PreferenceType.Track;
                return true;
            default:
                itemType = PreferenceType.Artist; // Or maybe PreferenceType.Unknown if you have that
                return false;
        }
    }
}