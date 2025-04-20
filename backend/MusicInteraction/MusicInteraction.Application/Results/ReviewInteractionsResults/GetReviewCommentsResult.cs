namespace MusicInteraction.Application;

public class GetReviewCommentsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<ReviewCommentDTO> Comments { get; set; }
    public int TotalCount { get; set; }
}