namespace MusicInteraction.Domain;

public class Like: Interaction
{

    public Like(Guid AggregateId, string ItemId, DateTime CreatedAt, string ItemType, string UserId)
        : base(AggregateId, ItemId, CreatedAt, ItemType, UserId) { }
}