using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class CheckSubscriptionStatusQueryHandler : IRequestHandler<CheckSubscriptionStatusQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<CheckSubscriptionStatusQueryHandler> _logger;
    private readonly IValidator<CheckSubscriptionStatusQuery> _validator;

    public CheckSubscriptionStatusQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<CheckSubscriptionStatusQueryHandler> logger,
        IValidator<CheckSubscriptionStatusQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<bool> Handle(CheckSubscriptionStatusQuery query, CancellationToken cancellationToken)
    {
        // Validate query
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if both users exist
        var follower = await _userRepository.GetByIdAsync(query.FollowerId);
        var followed = await _userRepository.GetByIdAsync(query.FollowedId);

        if (follower == null || followed == null)
        {
            _logger.LogWarning("One or both users not found: follower {FollowerId}, followed {FollowedId}",
                query.FollowerId, query.FollowedId);
            return false;
        }

        // Check subscription status
        var isFollowing = await _subscriptionRepository.IsFollowingAsync(query.FollowerId, query.FollowedId);

        _logger.LogInformation("User {FollowerId} is{Status} following user {FollowedId}",
            query.FollowerId, isFollowing ? "" : " not", query.FollowedId);

        return isFollowing;
    }
}