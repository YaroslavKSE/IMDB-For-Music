namespace MusicInteraction.Application;

public class GradingMethodSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
}