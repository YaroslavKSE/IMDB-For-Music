using MediatR;

namespace MusicInteraction.Application;

public class DeleteReviewCommentCommand : IRequest<DeleteReviewCommentResult>
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; }
}