using MediatR;

namespace MusicInteraction.Application;

public class CheckUserLikedReviewCommand : IRequest<CheckUserLikedReviewResult>
{
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
}