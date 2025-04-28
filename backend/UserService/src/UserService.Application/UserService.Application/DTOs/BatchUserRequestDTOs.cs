namespace UserService.Application.DTOs;

public class BatchUserRequestDto
{
    public List<Guid> UserIds { get; set; } = new();
}

public class BatchUserResponseDto
{
    public List<PublicUserProfileDto> Users { get; set; } = new();
}

public class PublicUserProfileDto
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