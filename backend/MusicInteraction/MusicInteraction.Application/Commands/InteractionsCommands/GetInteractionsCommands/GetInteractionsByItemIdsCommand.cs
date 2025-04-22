using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByItemIdsCommand : IRequest<GetInteractionsResult>
{
    public List<string> ItemIds { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}