namespace UserService.Application.DTOs;

public class LoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string IdToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; }
}