using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionByIdCommand : IRequest<GetInteractionDetailResult>
{
    public Guid InteractionId { get; set; }
}