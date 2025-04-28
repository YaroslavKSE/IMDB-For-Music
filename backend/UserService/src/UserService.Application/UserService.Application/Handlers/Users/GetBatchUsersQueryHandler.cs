using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Users;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class GetBatchUsersQueryHandler : IRequestHandler<GetBatchUsersQuery, BatchUserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetBatchUsersQueryHandler> _logger;
    private readonly IValidator<GetBatchUsersQuery> _validator;

    public GetBatchUsersQueryHandler(
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetBatchUsersQueryHandler> logger,
        IValidator<GetBatchUsersQuery> validator)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<BatchUserResponseDto> Handle(GetBatchUsersQuery request, CancellationToken cancellationToken)
    {
        // Validate the query
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Processing batch user info request for {Count} users", request.UserIds.Count);

        // Get all users in a single query
        var userEntities = await _userRepository.GetUsersByIdsAsync(request.UserIds);

        // Prepare the results list
        var users = new List<PublicUserProfileDto>();

        // For each found user, get their follower and following counts
        foreach (var user in userEntities)
        {
            // Get follower and following counts
            var followerCount = await _subscriptionRepository.GetFollowersCountAsync(user.Id);
            var followingCount = await _subscriptionRepository.GetFollowingCountAsync(user.Id);

            users.Add(new PublicUserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt
            });
        }

        _logger.LogInformation("Retrieved information for {Count} users out of {Requested} requested",
            users.Count, request.UserIds.Count);

        return new BatchUserResponseDto {Users = users};
    }
}