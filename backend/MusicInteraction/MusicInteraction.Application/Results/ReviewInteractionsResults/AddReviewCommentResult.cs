namespace MusicInteraction.Application;

public class AddReviewCommentResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ReviewCommentDTO Comment { get; set; }
}