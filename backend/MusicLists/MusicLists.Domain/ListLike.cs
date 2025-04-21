namespace MusicLists.Domain;

public class ListLike
{
    public Guid LikeId { get; private set; }
    public Guid ListId { get; private set; }
    public string UserId { get; private set; }
    public DateTime LikedAt { get; private set; }

    public ListLike(Guid listId, string userId)
    {
        LikeId = Guid.NewGuid();
        ListId = listId;
        UserId = userId;
        LikedAt = DateTime.UtcNow;
    }

    public ListLike(Guid likeId, Guid listId, string userId, DateTime likedAt)
    {
        LikeId = likeId;
        ListId = listId;
        UserId = userId;
        LikedAt = likedAt;
    }
}