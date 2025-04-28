using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Subscriptions;

public class SubscribeToUserCommandHandler : IRequestHandler<SubscribeToUserCommand, SubscriptionResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<SubscribeToUserCommandHandler> _logger;
    private readonly IValidator<SubscribeToUserCommand> _validator;

    public SubscribeToUserCommandHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<SubscribeToUserCommandHandler> logger,
        IValidator<SubscribeToUserCommand> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<SubscriptionResponseDto> Handle(SubscribeToUserCommand command,
        CancellationToken cancellationToken)
    {
        // Validate command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if both users exist
        var follower = await _userRepository.GetByIdAsync(command.FollowerId);
        if (follower == null)
            throw new NotFoundException($"Follower with ID {command.FollowerId} not found");

        var followed = await _userRepository.GetByIdAsync(command.FollowedId);
        if (followed == null)
            throw new NotFoundException($"User with ID {command.FollowedId} not found");

        // Check if already following
        var existingSubscription =
            await _subscriptionRepository.GetSubscriptionAsync(command.FollowerId, command.FollowedId);
        if (existingSubscription != null)
            throw new AlreadyExistsException("User is already subscribed");

        // Create subscription
        var subscription = UserSubscription.Create(command.FollowerId, command.FollowedId);

        await _subscriptionRepository.AddAsync(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        _logger.LogInformation("User {FollowerId} subscribed to {FollowedId}", command.FollowerId, command.FollowedId);

        return new SubscriptionResponseDto
        {
            SubscriptionId = subscription.Id,
            FollowerId = subscription.FollowerId,
            FollowedId = subscription.FollowedId,
            CreatedAt = subscription.CreatedAt
        };
    }
}