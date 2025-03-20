using MediatR;

namespace MusicInteraction.Application;

public class GetRatingByIdCommand : IRequest<GetRatingDetailResult>
{
    public Guid RatingId { get; set; }
}