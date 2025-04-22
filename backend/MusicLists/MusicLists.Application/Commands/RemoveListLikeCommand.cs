using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class RemoveListLikeCommand : IRequest<RemoveListLikeResult>
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
}