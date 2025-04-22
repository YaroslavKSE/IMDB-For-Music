namespace MusicLists.Application.Results;

public class CreateMusicListResult
{
    public bool Success { get; set; }
    public Guid? ListId { get; set; }
    public string ErrorMessage { get; set; }
}