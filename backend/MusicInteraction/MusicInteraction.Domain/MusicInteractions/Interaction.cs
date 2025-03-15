namespace MusicInteraction.Domain;

public abstract class Interaction
{
    public Guid AggregateId { get; private set; }
    public string ItemId { get; private set; }
    public string UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string ItemType { get; private set; }

    public Interaction(Guid id, string itemId, DateTime createdAt, string itemType, string userId)
    {
        AggregateId = id;
        ItemId = itemId;
        CreatedAt = createdAt;
        ItemType = itemType;
        UserId = userId;
    }
}