using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.QueryHandlers;

public abstract class BaseUserQueryHandler
{
    protected readonly IUserRepository UserRepository;
    protected readonly ILogger Logger;

    protected BaseUserQueryHandler(
        IUserRepository userRepository,
        ILogger logger)
    {
        UserRepository = userRepository;
        Logger = logger;
    }

    protected async Task<UserResponse> GetUserResponseAsync(User user)
    {
        if (user == null) return null;

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

    protected async Task<UserResponse> GetUserByIdAsync(Guid userId)
    {
        Logger.LogInformation("Getting user profile for user ID: {UserId}", userId);
        var user = await UserRepository.GetByIdAsync(userId);

        if (user == null)
        {
            Logger.LogWarning("User not found for ID: {UserId}", userId);
            return null;
        }

        Logger.LogInformation("User profile found for ID: {UserId}", userId);
        return await GetUserResponseAsync(user);
    }

    protected async Task<UserResponse> GetUserByUsernameAsync(string username)
    {
        Logger.LogInformation("Getting user profile for username: {Username}", username);
        var user = await UserRepository.GetByUsernameAsync(username);

        if (user == null)
        {
            Logger.LogWarning("User not found for username: {Username}", username);
            return null;
        }

        Logger.LogInformation("User profile found for username: {Username}", username);
        return await GetUserResponseAsync(user);
    }

    protected async Task<UserResponse> GetUserByEmailAsync(string email)
    {
        Logger.LogInformation("Getting user profile for email: {Email}", email);
        var user = await UserRepository.GetByEmailAsync(email);

        if (user == null)
        {
            Logger.LogWarning("User not found for email: {Email}", email);
            return null;
        }

        Logger.LogInformation("User profile found for email: {Email}", email);
        return await GetUserResponseAsync(user);
    }

    protected async Task<UserResponse> GetUserByAuth0IdAsync(string auth0Id)
    {
        Logger.LogInformation("Getting user profile for Auth0 user ID: {Auth0UserId}", auth0Id);
        var user = await UserRepository.GetByAuth0IdAsync(auth0Id);

        if (user == null)
        {
            Logger.LogWarning("User not found for Auth0 user ID: {Auth0UserId}", auth0Id);
            return null;
        }

        Logger.LogInformation("User profile found for Auth0 user ID: {Auth0UserId}", auth0Id);
        return await GetUserResponseAsync(user);
    }
}