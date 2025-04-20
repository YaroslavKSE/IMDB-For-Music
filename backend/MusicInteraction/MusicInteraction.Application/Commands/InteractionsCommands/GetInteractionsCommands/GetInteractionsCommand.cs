using MediatR;

namespace MusicInteraction.Application;

public class GetInteractionsCommand: IRequest<GetInteractionsResult>
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}