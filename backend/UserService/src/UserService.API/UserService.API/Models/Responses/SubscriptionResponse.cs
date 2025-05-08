namespace UserService.API.Models.Responses;

public class SubscriptionResponse
{
    public Guid SubscriptionId { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserSubscriptionResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime SubscribedAt { get; set; }
}

public class PaginatedSubscriptionsResponse
{
    public List<UserSubscriptionResponse> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}