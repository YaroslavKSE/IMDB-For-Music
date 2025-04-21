namespace MusicLists.Application.Commands;

using MediatR;
using MusicLists.Application.Results;

public class GetListByIdCommand : IRequest<GetListDetailResult>
{
    public Guid ListId { get; set; }
}