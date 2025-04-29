using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class GetUserFollowerIdsQueryHandler : IRequestHandler<GetUserFollowerIdsQuery, FollowerIdsResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowerIdsQueryHandler> _logger;
    private readonly IValidator<GetUserFollowerIdsQuery> _validator;

    public GetUserFollowerIdsQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowerIdsQueryHandler> logger,
        IValidator<GetUserFollowerIdsQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<FollowerIdsResponseDto> Handle(GetUserFollowerIdsQuery query, CancellationToken cancellationToken)
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

        // Get follower IDs with pagination
        var followerIds = await _subscriptionRepository.GetFollowerIdsAsync(query.UserId, query.Page, query.PageSize);
        var totalCount = await _subscriptionRepository.GetFollowersCountAsync(query.UserId);

        var totalPages = (int) Math.Ceiling(totalCount / (double) query.PageSize);

        _logger.LogInformation("Retrieved {Count} follower IDs for user {UserId}, total: {Total}",
            followerIds.Count, query.UserId, totalCount);

        return new FollowerIdsResponseDto
        {
            FollowerIds = followerIds,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}