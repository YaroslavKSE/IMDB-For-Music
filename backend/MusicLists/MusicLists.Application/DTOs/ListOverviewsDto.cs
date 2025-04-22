namespace MusicLists.Application.DTOs;

public class ListOverviewDto
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public string ListType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ListName { get; set; }
    public string ListDescription { get; set; }
    public bool IsRanked { get; set; }
    public List<ListItemDto> PreviewItems { get; set; } = new List<ListItemDto>();
    public int TotalItems { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public float HotScore { get; set; }
}