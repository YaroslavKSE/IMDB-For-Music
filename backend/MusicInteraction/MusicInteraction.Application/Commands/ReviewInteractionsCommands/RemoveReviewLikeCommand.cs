using MediatR;

namespace MusicInteraction.Application;

public class RemoveReviewLikeCommand : IRequest<RemoveReviewLikeResult>
{
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
}