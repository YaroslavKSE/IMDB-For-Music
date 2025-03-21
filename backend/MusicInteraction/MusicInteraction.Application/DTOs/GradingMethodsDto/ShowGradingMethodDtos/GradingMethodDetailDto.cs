namespace MusicInteraction.Application;

public class GradingMethodDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public List<GradableComponentShowDto> Components { get; set; }
    public List<string> Actions { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}