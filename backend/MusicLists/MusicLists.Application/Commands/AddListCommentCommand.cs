using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class AddListCommentCommand : IRequest<AddListCommentResult>
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public string CommentText { get; set; }
}