using MediatR;

namespace MusicInteraction.Application;

public class GetItemStatsByIdCommand : IRequest<GetItemStatsResult>
{
    public string ItemId { get; set; }
}