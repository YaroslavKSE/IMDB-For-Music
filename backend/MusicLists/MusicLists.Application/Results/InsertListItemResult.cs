namespace MusicLists.Application.Results;

public class InsertListItemResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int NewPosition { get; set; }
    public int TotalItems { get; set; }
}