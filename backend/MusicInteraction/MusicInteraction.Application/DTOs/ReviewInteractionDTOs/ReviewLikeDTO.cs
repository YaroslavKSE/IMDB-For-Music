namespace MusicInteraction.Application;

public class ReviewLikeDTO
{
    public Guid LikeId { get; set; }
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
    public DateTime LikedAt { get; set; }
}