namespace MusicInteraction.Domain;

public abstract class Interaction
{
    private Guid InteractionId;
    private string ItemId;
    private DateTime CreatedAt;
    private string ItemType;

    public Interaction(Guid Id, string ItemId, DateTime CreatedAt, string ItemType)
    {
        InteractionId = Id;
        this.ItemType = ItemId;
        this.CreatedAt = CreatedAt;
        this.ItemType = ItemType;
    }
}