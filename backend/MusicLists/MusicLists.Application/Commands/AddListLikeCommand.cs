using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class AddListLikeCommand : IRequest<AddListLikeResult>
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
}