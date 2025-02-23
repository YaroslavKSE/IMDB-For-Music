namespace MusicInteraction.Domain;

public class Checked(Guid InteractionId, string ItemId, DateTime CreatedAt, string ItemType)
    : Interaction(InteractionId, ItemId, CreatedAt, ItemType);