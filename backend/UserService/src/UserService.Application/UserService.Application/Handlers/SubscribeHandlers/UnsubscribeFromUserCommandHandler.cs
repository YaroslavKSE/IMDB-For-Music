using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class UnsubscribeFromUserCommandHandler : IRequestHandler<UnsubscribeFromUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<UnsubscribeFromUserCommandHandler> _logger;
    private readonly IValidator<UnsubscribeFromUserCommand> _validator;

    public UnsubscribeFromUserCommandHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<UnsubscribeFromUserCommandHandler> logger,
        IValidator<UnsubscribeFromUserCommand> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<bool> Handle(UnsubscribeFromUserCommand command, CancellationToken cancellationToken)
    {
        // Validate command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Verify both users exist
        var follower = await _userRepository.GetByIdAsync(command.FollowerId);
        if (follower == null)
        {
            _logger.LogWarning("Follower with ID {FollowerId} not found", command.FollowerId);
            return false;
        }

        var followed = await _userRepository.GetByIdAsync(command.FollowedId);
        if (followed == null)
        {
            _logger.LogWarning("User with ID {FollowedId} not found", command.FollowedId);
            return false;
        }

        // Check if the subscription exists
        var subscription = await _subscriptionRepository.GetSubscriptionAsync(command.FollowerId, command.FollowedId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for follower {FollowerId} and followed {FollowedId}",
                command.FollowerId, command.FollowedId);
            return false;
        }

        // Remove the subscription
        await _subscriptionRepository.RemoveAsync(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        _logger.LogInformation("User {FollowerId} unsubscribed from {FollowedId}",
            command.FollowerId, command.FollowedId);

        return true;
    }
}