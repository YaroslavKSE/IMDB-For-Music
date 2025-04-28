namespace UserService.API.Models.Requests;

public class BatchUserRequest
{
    public List<Guid> UserIds { get; set; } = new();
}