using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class CheckUserLikedListCommand : IRequest<CheckUserLikedListResult>
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
}