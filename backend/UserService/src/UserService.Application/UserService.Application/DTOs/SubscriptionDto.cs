namespace UserService.Application.DTOs;

public class SubscriptionResponseDto
{
    public Guid SubscriptionId { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserSubscriptionDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime SubscribedAt { get; set; }
}

public class PaginatedSubscriptionsResponseDto
{
    public List<UserSubscriptionDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}