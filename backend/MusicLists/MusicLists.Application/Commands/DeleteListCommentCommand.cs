using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class DeleteListCommentCommand : IRequest<DeleteListCommentResult>
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; }
}