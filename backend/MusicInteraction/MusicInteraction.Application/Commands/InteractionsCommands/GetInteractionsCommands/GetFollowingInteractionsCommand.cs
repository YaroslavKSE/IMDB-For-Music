using MediatR;

namespace MusicInteraction.Application;

public class GetFollowingInteractionsCommand : IRequest<GetInteractionsResult>
{
    public string UserId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}