namespace MusicInteraction.Domain;

public abstract class Interaction
{
    public Guid InteractionId { get; private set; }
    public string ItemId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string ItemType { get; private set; }

    public Interaction(Guid id, string itemId, DateTime createdAt, string itemType)
    {
        InteractionId = id;
        ItemId = itemId;
        CreatedAt = createdAt;
        ItemType = itemType;
    }
}