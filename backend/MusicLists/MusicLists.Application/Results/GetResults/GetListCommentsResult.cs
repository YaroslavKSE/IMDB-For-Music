namespace MusicLists.Application.Results;

using MusicLists.Application.DTOs;

public class GetListCommentsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<ListCommentDto> Comments { get; set; }
    public int TotalCount { get; set; }
}