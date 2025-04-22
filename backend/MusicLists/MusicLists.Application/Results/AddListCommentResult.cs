using MusicLists.Application.DTOs;

namespace MusicLists.Application.Results;

public class AddListCommentResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ListCommentDto Comment { get; set; }
}