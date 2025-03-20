namespace MusicInteraction.Application;

public class GetRatingDetailResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public RatingDetailDTO Rating { get; set; }
}