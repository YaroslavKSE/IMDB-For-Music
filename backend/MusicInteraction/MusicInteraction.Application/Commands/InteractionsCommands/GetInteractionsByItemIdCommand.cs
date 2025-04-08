using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByItemIdCommand : IRequest<GetInteractionsResult>
{
    public string ItemId { get; set; }
}