namespace MusicLists.Application.Results;

using MusicLists.Application.DTOs;

public class GetListsOverviewResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<ListOverviewDto> Lists { get; set; }
    public int TotalCount { get; set; }
}