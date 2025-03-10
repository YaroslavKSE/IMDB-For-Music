namespace MusicInteraction.Domain;

public class Review: Interaction
{
    private readonly Guid ReviewId;
    private string ReviewText;

    public Review(string text, Guid AggregateId, string ItemId, DateTime CreatedAt, string ItemType, string UserId)
        : base(AggregateId, ItemId, CreatedAt, ItemType, UserId)
    {
        ReviewId = Guid.NewGuid();
        ReviewText = text;
    }

    public void updateReview(string text)
    {
        ReviewText = text;
    }

    public string GetText()
    {
        return ReviewText;
    }

    public Guid getReviewId()
    {
        return ReviewId;
    }
}