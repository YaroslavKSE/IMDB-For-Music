using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class GetUserFollowingQueryHandler : IRequestHandler<GetUserFollowingQuery, PaginatedSubscriptionsResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowingQueryHandler> _logger;
    private readonly IValidator<GetUserFollowingQuery> _validator;

    public GetUserFollowingQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowingQueryHandler> logger,
        IValidator<GetUserFollowingQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<PaginatedSubscriptionsResponseDto> Handle(GetUserFollowingQuery query,
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

        // Get following users with pagination
        var following = await _subscriptionRepository.GetFollowingAsync(query.UserId, query.Page, query.PageSize);
        var totalCount = await _subscriptionRepository.GetFollowingCountAsync(query.UserId);

        // Map to response DTO
        var items = following.Select(s => new UserSubscriptionDto
        {
            UserId = s.FollowedId,
            Username = s.Followed.Username,
            Name = s.Followed.Name,
            Surname = s.Followed.Surname,
            AvatarUrl = s.Followed.AvatarUrl,
            SubscribedAt = s.CreatedAt
        }).ToList();

        var totalPages = (int) Math.Ceiling(totalCount / (double) query.PageSize);

        _logger.LogInformation("Retrieved {Count} following for user {UserId}, total: {Total}",
            following.Count, query.UserId, totalCount);

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