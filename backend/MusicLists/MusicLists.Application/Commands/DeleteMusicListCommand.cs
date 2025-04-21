using MediatR;
using MusicLists.Application.Results;

namespace MusicLists.Application.Commands;

public class DeleteMusicListCommand : IRequest<DeleteMusicListResult>
{
    public Guid ListId { get; set; }
}