using MediatR;
using MusicLists.Application;
using MusicLists.Application.Results;
using System.Collections.Generic;
using MusicLists.Application.DTOs;

namespace MusicLists.Application.Commands;

public class CreateMusicListCommand : IRequest<CreateMusicListResult>
{
    public string UserId { get; set; }
    public string ListType { get; set; }
    public string ListName { get; set; }
    public string ListDescription { get; set; }
    public bool IsRanked { get; set; }
    public List<ListItemDto> Items { get; set; } = new List<ListItemDto>();
}