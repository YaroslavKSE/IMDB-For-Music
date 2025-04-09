namespace UserService.API.Models.Responses;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    public string Username { get; set; }
    // Additional properties can be added later (bio, preferences, etc.)
}