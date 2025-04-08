namespace MusicInteraction.Application;

public class RatingNormalizedDTO
{
    public Guid RatingId { get; set; }
    public float? NormalizedGrade { get; set; }
    public bool? IsComplex { get; set; }
}