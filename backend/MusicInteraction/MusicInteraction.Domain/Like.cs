namespace MusicInteraction.Domain;

public class Like(Guid InteractionId, string ItemId, DateTime CreatedAt, string ItemType)
    : Interaction(InteractionId, ItemId, CreatedAt, ItemType);