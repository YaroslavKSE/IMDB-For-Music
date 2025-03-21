namespace MusicInteraction.Application;

public class GradeDetailShowDto : GradableComponentShowDto
{
    public float StepAmount { get; set; }
    public string? Description { get; set; }
}