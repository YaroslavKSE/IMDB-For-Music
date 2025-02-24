namespace MusicInteraction.Domain;

public class Review: Interaction
{
    private Guid ReviewId;
    private string ReviewText;

    public Review(string text, Guid InteractionId, string ItemId, DateTime CreatedAt, string ItemType)
        : base(InteractionId, ItemId, CreatedAt, ItemType)
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