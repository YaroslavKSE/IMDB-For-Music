namespace MusicLists.Application.Results;

using MusicLists.Application.DTOs;

public class AddListLikeResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ListLikeDto Like { get; set; }
}