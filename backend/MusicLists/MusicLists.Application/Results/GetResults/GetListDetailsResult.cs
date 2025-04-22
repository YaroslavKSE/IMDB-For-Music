namespace MusicLists.Application.Results;

using MusicLists.Application.DTOs;

public class GetListDetailResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ListDetailDto List { get; set; }
}