using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Queries.Subscriptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class GetUserFollowingCountQueryHandler : IRequestHandler<GetUserFollowingCountQuery, int>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<GetUserFollowingCountQueryHandler> _logger;

    public GetUserFollowingCountQueryHandler(
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<GetUserFollowingCountQueryHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<int> Handle(GetUserFollowingCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _subscriptionRepository.GetFollowingCountAsync(request.UserId);
        _logger.LogInformation("Retrieved following count for user {UserId}: {Count}", request.UserId, count);
        return count;
    }
}