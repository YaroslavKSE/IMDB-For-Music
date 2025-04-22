namespace MusicLists.Application.Commands;

using MediatR;
using MusicLists.Application.Results;

public class GetListsByUserIdCommand : IRequest<GetListsOverviewResult>
{
    public string UserId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public string? ListType { get; set; }
}