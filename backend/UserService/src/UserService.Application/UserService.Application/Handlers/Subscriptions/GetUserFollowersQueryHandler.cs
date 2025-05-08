using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class GetUserFollowersQueryHandler : IRequestHandler<GetUserFollowersQuery, PaginatedSubscriptionsResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowersQueryHandler> _logger;
    private readonly IValidator<GetUserFollowersQuery> _validator;

    public GetUserFollowersQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowersQueryHandler> logger,
        IValidator<GetUserFollowersQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<PaginatedSubscriptionsResponseDto> Handle(GetUserFollowersQuery query,
        CancellationToken cancellationToken)
    {
        // Validate query
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if user exists
        var user = await _userRepository.GetByIdAsync(query.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found with ID {UserId}", query.UserId);
            throw new Domain.Exceptions.NotFoundException($"User with ID {query.UserId} not found");
        }

        // Get followers with pagination
        var followers = await _subscriptionRepository.GetFollowersAsync(query.UserId, query.Page, query.PageSize);
        var totalCount = await _subscriptionRepository.GetFollowersCountAsync(query.UserId);

        // Map to response DTO
        var items = followers.Select(s => new UserSubscriptionDto
        {
            UserId = s.FollowerId,
            Username = s.Follower.Username,
            Name = s.Follower.Name,
            Surname = s.Follower.Surname,
            AvatarUrl = s.Follower.AvatarUrl,
            SubscribedAt = s.CreatedAt
        }).ToList();

        var totalPages = (int) Math.Ceiling(totalCount / (double) query.PageSize);

        _logger.LogInformation("Retrieved {Count} followers for user {UserId}, total: {Total}",
            followers.Count, query.UserId, totalCount);

        return new PaginatedSubscriptionsResponseDto
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}