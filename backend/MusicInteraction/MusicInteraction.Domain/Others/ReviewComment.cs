namespace MusicInteraction.Domain;

public class ReviewComment
{
    public Guid CommentId { get; private set; }
    public Guid ReviewId { get; private set; }
    public string UserId { get; private set; }
    public DateTime CommentedAt { get; private set; }
    public string CommentText { get; private set; }

    public ReviewComment(Guid reviewId, string userId, string commentText)
    {
        CommentId = Guid.NewGuid();
        ReviewId = reviewId;
        UserId = userId;
        CommentedAt = DateTime.UtcNow;
        CommentText = commentText;
    }

    // Used by mapping methods
    public ReviewComment(Guid commentId, Guid reviewId, string userId, DateTime commentedAt, string commentText)
    {
        CommentId = commentId;
        ReviewId = reviewId;
        UserId = userId;
        CommentedAt = commentedAt;
        CommentText = commentText;
    }

    public void UpdateComment(string newText)
    {
        CommentText = newText;
    }
}