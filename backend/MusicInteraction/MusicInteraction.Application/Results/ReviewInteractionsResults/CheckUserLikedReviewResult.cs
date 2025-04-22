namespace MusicInteraction.Application;

public class CheckUserLikedReviewResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public bool HasLiked { get; set; }
}