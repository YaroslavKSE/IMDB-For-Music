using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByUserAndItemCommand : IRequest<GetInteractionsResult>
{
    public string UserId { get; set; }
    public string ItemId { get; set; }
}