using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByUserIdCommand : IRequest<GetInteractionsResult>
{
    public string UserId { get; set; }
}