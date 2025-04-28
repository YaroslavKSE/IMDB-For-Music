using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class GetUserFollowersCountQueryHandler : IRequestHandler<GetUserFollowersCountQuery, int>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowersCountQueryHandler> _logger;

    public GetUserFollowersCountQueryHandler(
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowersCountQueryHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<int> Handle(GetUserFollowersCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _subscriptionRepository.GetFollowersCountAsync(request.UserId);
        _logger.LogInformation("Retrieved follower count for user {UserId}: {Count}", request.UserId, count);
        return count;
    }
}