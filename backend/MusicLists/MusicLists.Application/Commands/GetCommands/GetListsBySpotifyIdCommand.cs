namespace MusicLists.Application.Commands;

using MediatR;
using MusicLists.Application.Results;

public class GetListsBySpotifyIdCommand : IRequest<GetListsOverviewResult>
{
    public string SpotifyId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}