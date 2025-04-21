namespace MusicLists.Application.Commands;

using MediatR;
using MusicLists.Application.Results;

public class GetListCommentsCommand : IRequest<GetListCommentsResult>
{
    public Guid ListId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}