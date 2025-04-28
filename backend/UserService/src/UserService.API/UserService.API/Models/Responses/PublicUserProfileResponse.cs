using System;

public class PublicUserProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public string AvatarUrl { get; set; }
    public string Bio { get; set; } 
    public DateTime CreatedAt { get; set; }
}