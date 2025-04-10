namespace UserService.API.Models.Requests;

public class UpdateUserProfileRequest
{
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
}