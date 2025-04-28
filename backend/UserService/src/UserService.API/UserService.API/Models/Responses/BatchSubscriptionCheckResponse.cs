namespace UserService.API.Models.Responses;

public class BatchSubscriptionCheckResponse
{
    public Dictionary<Guid, bool> Results { get; set; } = new();
}