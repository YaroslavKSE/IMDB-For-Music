namespace UserService.Application.DTOs;

public class BatchSubscriptionRequestDto
{
    public List<Guid> TargetUserIds { get; set; } = new();
}

public class BatchSubscriptionResponseDto
{
    public Dictionary<Guid, bool> Results { get; set; } = new();
}