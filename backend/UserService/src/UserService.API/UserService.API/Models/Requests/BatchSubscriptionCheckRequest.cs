namespace UserService.API.Models.Requests;

public class BatchSubscriptionCheckRequest
{
    public List<Guid> TargetUserIds { get; set; } = new();
}