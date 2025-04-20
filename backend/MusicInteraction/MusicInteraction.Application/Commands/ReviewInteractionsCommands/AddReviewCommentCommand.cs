using MediatR;

namespace MusicInteraction.Application;

public class AddReviewCommentCommand : IRequest<AddReviewCommentResult>
{
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
    public string CommentText { get; set; }
}