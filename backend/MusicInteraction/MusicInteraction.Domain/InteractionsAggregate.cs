namespace MusicInteraction.Domain;

public class InteractionsAggregate
{
    public Guid AggregateId { get; private set; }
    public string UserId { get; private set; }
    public string ItemId { get; private set; }
    public string ItemType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public virtual Rating? Rating { get; private set; }
    public virtual Review? Review { get; private set; }
    public bool IsLiked { get; private set; }

    public InteractionsAggregate(string userId, string itemId, string itemType)
    {
        AggregateId = Guid.NewGuid();
        UserId = userId;
        ItemId = itemId;
        ItemType = itemType;
        CreatedAt = DateTime.UtcNow;
        IsLiked = false;
    }

    public void AddRating(IGradable grade)
    {
        Rating = new Rating(grade, AggregateId, ItemId, CreatedAt, ItemType, UserId);
    }

    public void AddReview(string text)
    {
        Review = new Review(text, AggregateId, ItemId, CreatedAt, ItemType, UserId);
    }

    public void AddLike()
    {
        IsLiked = true;
    }

    public void RemoveRating()
    {
        Rating = null;
    }

    public void RemoveReview()
    {
        Review = null;
    }

    public void Unlike()
    {
        IsLiked = false;
    }
}