namespace MusicLists.Application.DTOs;

public class ListCommentDto
{
    public Guid CommentId { get; set; }
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public DateTime CommentedAt { get; set; }
    public string CommentText { get; set; }
}