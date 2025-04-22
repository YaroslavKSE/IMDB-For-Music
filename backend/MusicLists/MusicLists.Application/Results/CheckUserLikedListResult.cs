namespace MusicLists.Application.Results;

public class CheckUserLikedListResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public bool HasLiked { get; set; }
}