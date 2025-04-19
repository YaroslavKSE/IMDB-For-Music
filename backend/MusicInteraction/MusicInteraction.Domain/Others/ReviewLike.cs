namespace MusicInteraction.Domain;

public class ReviewLike
{
    public Guid LikeId { get; private set; }
    public Guid ReviewId { get; private set; }
    public string UserId { get; private set; }
    public DateTime LikedAt { get; private set; }

    public ReviewLike(Guid reviewId, string userId)
    {
        LikeId = Guid.NewGuid();
        ReviewId = reviewId;
        UserId = userId;
        LikedAt = DateTime.UtcNow;
    }

    // Used by mapping methods
    public ReviewLike(Guid likeId, Guid reviewId, string userId, DateTime likedAt)
    {
        LikeId = likeId;
        ReviewId = reviewId;
        UserId = userId;
        LikedAt = likedAt;
    }
}