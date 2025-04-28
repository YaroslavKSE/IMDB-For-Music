using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class CheckBatchSubscriptionStatusQueryHandler : IRequestHandler<CheckBatchSubscriptionStatusQuery, BatchSubscriptionResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<CheckBatchSubscriptionStatusQueryHandler> _logger;
    private readonly IValidator<CheckBatchSubscriptionStatusQuery> _validator;

    public CheckBatchSubscriptionStatusQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<CheckBatchSubscriptionStatusQueryHandler> logger,
        IValidator<CheckBatchSubscriptionStatusQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<BatchSubscriptionResponseDto> Handle(CheckBatchSubscriptionStatusQuery query, CancellationToken cancellationToken)
    {
        // Validate query
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Processing batch subscription check for user {FollowerId} with {Count} target users",
            query.FollowerId, query.TargetUserIds.Count);

        // Verify follower user exists
        var follower = await _userRepository.GetByIdAsync(query.FollowerId);
        if (follower == null)
        {
            _logger.LogWarning("Follower with ID {FollowerId} not found", query.FollowerId);
            throw new Domain.Exceptions.NotFoundException($"User with ID {query.FollowerId} not found");
        }

        // For non-existent users and self-references, initialize results dictionary
        var results = new Dictionary<Guid, bool>();
        
        // Handle self-references: user cannot follow themselves
        foreach (var targetId in query.TargetUserIds.Where(id => id == query.FollowerId))
        {
            results[targetId] = false;
        }
        
        // Filter out self-references for the database query
        var filteredTargetIds = query.TargetUserIds
            .Where(id => id != query.FollowerId)
            .ToList();
            
        if (filteredTargetIds.Count > 0)
        {
            // Check which target users exist
            var existingUserIds = await _userRepository.GetExistingUserIdsAsync(filteredTargetIds);
            
            // Mark non-existent users as not followed
            foreach (var targetId in filteredTargetIds.Where(id => !existingUserIds.Contains(id)))
            {
                results[targetId] = false;
            }
            
            // For existing users, check subscription status in a single operation
            var existingTargetIds = filteredTargetIds
                .Where(id => existingUserIds.Contains(id))
                .ToList();
                
            if (existingTargetIds.Count > 0)
            {
                var subscriptionResults = await _subscriptionRepository.AreBatchFollowingAsync(
                    query.FollowerId, existingTargetIds);
                    
                // Merge results
                foreach (var kvp in subscriptionResults)
                {
                    results[kvp.Key] = kvp.Value;
                }
            }
        }

        _logger.LogInformation("Completed batch subscription check for follower {FollowerId}", query.FollowerId);

        return new BatchSubscriptionResponseDto { Results = results };
    }
}