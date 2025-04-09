using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user profile for Auth0 user ID: {Auth0UserId}", request.Auth0UserId);

        var user = await _userRepository.GetByAuth0IdAsync(request.Auth0UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found for Auth0 user ID: {Auth0UserId}", request.Auth0UserId);
            return null;
        }

        _logger.LogInformation("User profile found for Auth0 user ID: {Auth0UserId}", request.Auth0UserId);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Name = user.Name,
            Surname = user.Surname,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}