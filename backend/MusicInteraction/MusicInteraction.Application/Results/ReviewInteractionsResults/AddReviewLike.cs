namespace MusicInteraction.Application;

public class AddReviewLikeResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ReviewLikeDTO Like { get; set; }
}