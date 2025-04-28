using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Preferences;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Preferences;

public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPreferenceRepository _preferenceRepository;
    private readonly ILogger<GetUserPreferencesQueryHandler> _logger;
    private readonly IValidator<GetUserPreferencesQuery> _validator;

    public GetUserPreferencesQueryHandler(
        IUserRepository userRepository,
        IUserPreferenceRepository preferenceRepository,
        ILogger<GetUserPreferencesQueryHandler> logger,
        IValidator<GetUserPreferencesQuery> validator)
    {
        _userRepository = userRepository;
        _preferenceRepository = preferenceRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserPreferencesResponse> Handle(GetUserPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        // Validate the query
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Get user by Auth0 ID
        var user = await _userRepository.GetByAuth0IdAsync(request.Auth0UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found with Auth0 ID: {Auth0UserId}", request.Auth0UserId);
            throw new Domain.Exceptions.NotFoundException($"User with Auth0 ID {request.Auth0UserId} not found");
        }

        // Get user preferences grouped by type
        var preferences = await _preferenceRepository.GetUserPreferencesAsync(user.Id);

        // Map to response
        var response = new UserPreferencesResponse
        {
            Artists = preferences.TryGetValue(PreferenceType.Artist, out var artists) ? artists : new List<string>(),
            Albums = preferences.TryGetValue(PreferenceType.Album, out var albums) ? albums : new List<string>(),
            Tracks = preferences.TryGetValue(PreferenceType.Track, out var tracks) ? tracks : new List<string>()
        };

        _logger.LogInformation(
            "Retrieved preferences for user {UserId}: {ArtistCount} artists, {AlbumCount} albums, {TrackCount} tracks",
            user.Id, response.Artists.Count, response.Albums.Count, response.Tracks.Count);

        return response;
    }
}