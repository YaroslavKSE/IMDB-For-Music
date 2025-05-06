namespace MusicLists.Application.Commands;

using MediatR;
using MusicLists.Application.Results;

public class InsertListItemCommand : IRequest<InsertListItemResult>
{
    public Guid ListId { get; set; }
    public string SpotifyId { get; set; }
    public int? Position { get; set; }
}