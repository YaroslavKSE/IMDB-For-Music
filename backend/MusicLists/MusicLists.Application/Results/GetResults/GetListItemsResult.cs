namespace MusicLists.Application.Results;

using MusicLists.Application.DTOs;

public class GetListItemsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<ListItemDto> Items { get; set; }
    public int TotalCount { get; set; }
}