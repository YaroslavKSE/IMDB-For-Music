using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsByUserIdsCommand : IRequest<GetInteractionsResult>
{
    public List<string> UserIds { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}