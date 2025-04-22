namespace MusicLists.Domain;

public class ListComment
{
    public Guid CommentId { get; private set; }
    public Guid ListId { get; private set; }
    public string UserId { get; private set; }
    public DateTime CommentedAt { get; private set; }
    public string CommentText { get; private set; }

    public ListComment(Guid listId, string userId, string commentText)
    {
        CommentId = Guid.NewGuid();
        ListId = listId;
        UserId = userId;
        CommentedAt = DateTime.UtcNow;
        CommentText = commentText;
    }

    public ListComment(Guid commentId, Guid listId, string userId, DateTime commentedAt, string commentText)
    {
        CommentId = commentId;
        ListId = listId;
        UserId = userId;
        CommentedAt = commentedAt;
        CommentText = commentText;
    }

    public void UpdateComment(string newText)
    {
        CommentText = newText;
    }
}