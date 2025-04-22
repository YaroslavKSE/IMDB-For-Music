namespace MusicInteraction.Application;

public class ReviewCommentDTO
{
    public Guid CommentId { get; set; }
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
    public DateTime CommentedAt { get; set; }
    public string CommentText { get; set; }
}