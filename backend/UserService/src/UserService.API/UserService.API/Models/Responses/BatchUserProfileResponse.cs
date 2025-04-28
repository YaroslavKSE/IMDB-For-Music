namespace UserService.API.Models.Responses;

public class BatchUserResponse
{
    public List<BatchUserProfileResponse> Users { get; set; } = new();
}

public class BatchUserProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}