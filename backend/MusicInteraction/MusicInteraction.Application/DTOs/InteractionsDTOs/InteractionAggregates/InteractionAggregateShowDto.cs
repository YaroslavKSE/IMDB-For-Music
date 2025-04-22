namespace MusicInteraction.Application;

public class InteractionAggregateShowDto
{
    public Guid AggregateId { get; set; }
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual RatingNormalizedDTO? Rating { get; set; }
    public virtual ReviewDTO? Review { get; set; }
    public bool IsLiked { get; set; }
}