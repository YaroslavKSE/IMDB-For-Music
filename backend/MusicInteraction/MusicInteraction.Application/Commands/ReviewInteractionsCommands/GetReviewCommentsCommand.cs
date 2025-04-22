using MediatR;

namespace MusicInteraction.Application;

public class GetReviewCommentsCommand : IRequest<GetReviewCommentsResult>
{
    public Guid ReviewId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}