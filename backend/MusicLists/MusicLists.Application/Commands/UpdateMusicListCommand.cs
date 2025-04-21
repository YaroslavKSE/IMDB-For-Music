using MediatR;
using MusicLists.Application.DTOs;
using MusicLists.Application.Results;
using System;
using System.Collections.Generic;

namespace MusicLists.Application.Commands;

public class UpdateMusicListCommand : IRequest<UpdateMusicListResult>
{
    public Guid ListId { get; set; }
    public string ListType { get; set; }
    public string ListName { get; set; }
    public string ListDescription { get; set; }
    public bool IsRanked { get; set; }
    public List<ListItemDto> Items { get; set; } = new List<ListItemDto>();
}