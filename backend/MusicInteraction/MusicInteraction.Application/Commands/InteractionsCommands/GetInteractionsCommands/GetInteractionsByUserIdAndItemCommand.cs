using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByUserAndItemCommand : IRequest<GetInteractionsResult>
{
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}