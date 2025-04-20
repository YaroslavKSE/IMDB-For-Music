namespace MusicInteraction.Application;

public class RatingDetailDTO
{
    public Guid RatingId { get; set; }
    public Guid? GradingMethodId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public float? NormalizedGrade { get; set; }
    public float? OverallGrade { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
    public GradedComponentDTO GradingComponent { get; set; }
}