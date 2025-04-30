using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class GetUserFollowingIdsQueryHandler : IRequestHandler<GetUserFollowingIdsQuery, FollowingIdsResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowingIdsQueryHandler> _logger;
    private readonly IValidator<GetUserFollowingIdsQuery> _validator;

    public GetUserFollowingIdsQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowingIdsQueryHandler> logger,
        IValidator<GetUserFollowingIdsQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<FollowingIdsResponseDto> Handle(GetUserFollowingIdsQuery query,
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

        // Get following IDs with pagination
        var followingIds = await _subscriptionRepository.GetFollowingIdsAsync(query.UserId, query.Page, query.PageSize);
        var totalCount = await _subscriptionRepository.GetFollowingCountAsync(query.UserId);

        var totalPages = (int) Math.Ceiling(totalCount / (double) query.PageSize);

        _logger.LogInformation("Retrieved {Count} following IDs for user {UserId}, total: {Total}",
            followingIds.Count, query.UserId, totalCount);

        return new FollowingIdsResponseDto
        {
            FollowingIds = followingIds,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}