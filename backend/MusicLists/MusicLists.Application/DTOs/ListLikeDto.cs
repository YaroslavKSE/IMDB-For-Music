namespace MusicLists.Application.DTOs;

public class ListLikeDto
{
    public Guid LikeId { get; set; }
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public DateTime LikedAt { get; set; }
}