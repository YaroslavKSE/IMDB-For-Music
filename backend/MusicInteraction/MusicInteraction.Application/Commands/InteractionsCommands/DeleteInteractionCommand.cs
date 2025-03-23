using MediatR;

namespace MusicInteraction.Application;

public class DeleteInteractionCommand : IRequest<DeleteInteractionResult>
{
    public Guid InteractionId { get; set; }
}