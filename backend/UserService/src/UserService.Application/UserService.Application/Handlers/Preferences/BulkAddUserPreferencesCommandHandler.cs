using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Queries.Preferences;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Preferences;

public class BulkAddUserPreferencesCommandHandler : IRequestHandler<BulkAddUserPreferencesCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPreferenceRepository _preferenceRepository;
    private readonly ILogger<BulkAddUserPreferencesCommandHandler> _logger;
    private readonly IValidator<BulkAddUserPreferencesCommand> _validator;

    public BulkAddUserPreferencesCommandHandler(
        IUserRepository userRepository,
        IUserPreferenceRepository preferenceRepository,
        ILogger<BulkAddUserPreferencesCommandHandler> logger,
        IValidator<BulkAddUserPreferencesCommand> validator)
    {
        _userRepository = userRepository;
        _preferenceRepository = preferenceRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<bool> Handle(BulkAddUserPreferencesCommand request, CancellationToken cancellationToken)
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

        // Create a collection to hold all preferences to add
        var preferencesToAdd = new List<UserPreference>();

        // Check existing preferences to avoid duplicates
        var existingPreferences = await _preferenceRepository.GetUserPreferencesAsync(user.Id);

        // Process artists
        foreach (var artistId in request.Artists)
            if (!existingPreferences.TryGetValue(PreferenceType.Artist, out var existingArtists) ||
                !existingArtists.Contains(artistId))
                preferencesToAdd.Add(UserPreference.Create(user.Id, PreferenceType.Artist, artistId));

        // Process albums
        foreach (var albumId in request.Albums)
            if (!existingPreferences.TryGetValue(PreferenceType.Album, out var existingAlbums) ||
                !existingAlbums.Contains(albumId))
                preferencesToAdd.Add(UserPreference.Create(user.Id, PreferenceType.Album, albumId));

        // Process tracks
        foreach (var trackId in request.Tracks)
            if (!existingPreferences.TryGetValue(PreferenceType.Track, out var existingTracks) ||
                !existingTracks.Contains(trackId))
                preferencesToAdd.Add(UserPreference.Create(user.Id, PreferenceType.Track, trackId));

        // Add all preferences in one batch if any exist to add
        if (preferencesToAdd.Count > 0)
        {
            await _preferenceRepository.AddRangeAsync(preferencesToAdd);
            await _preferenceRepository.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Bulk added preferences for user {UserId}: {ArtistCount} artists, {AlbumCount} albums, {TrackCount} tracks",
            user.Id, request.Artists.Count, request.Albums.Count, request.Tracks.Count);

        return true;
    }
}