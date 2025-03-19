namespace UserService.API.Models.Requests;

public class SocialLoginRequest
{
    public string AccessToken { get; set; }
    public string Provider { get; set; }
}