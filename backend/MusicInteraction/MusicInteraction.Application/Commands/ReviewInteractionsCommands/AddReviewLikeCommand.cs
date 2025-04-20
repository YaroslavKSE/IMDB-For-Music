using MediatR;

namespace MusicInteraction.Application;

public class AddReviewLikeCommand : IRequest<AddReviewLikeResult>
{
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
}