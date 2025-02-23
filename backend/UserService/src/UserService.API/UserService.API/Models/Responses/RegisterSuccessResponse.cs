namespace UserService.API.Models.Responses;

public class RegisterSuccessResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; }
}