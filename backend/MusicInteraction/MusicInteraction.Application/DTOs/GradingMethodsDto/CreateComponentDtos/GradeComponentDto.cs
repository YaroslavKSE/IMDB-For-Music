namespace MusicInteraction.Application;

public class GradeComponentDto : ComponentDto
{
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
    public float StepAmount { get; set; }
}