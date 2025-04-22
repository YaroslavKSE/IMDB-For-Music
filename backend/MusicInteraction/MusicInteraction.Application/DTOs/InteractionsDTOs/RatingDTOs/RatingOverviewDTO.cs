namespace MusicInteraction.Application;

public class RatingOverviewDTO
{
    public Guid RatingId { get; set; }
    public float? NormalizedGrade { get; set; }
    public float? Grade { get; set; }
    public float? MinGrade { get; set; }
    public float? MaxGrade { get; set; }
}