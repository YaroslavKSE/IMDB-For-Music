namespace MusicInteraction.Domain;

public class Rating: Interaction
{
    public Guid RatingId { get; set; }
    public IGradable Grade { get; set; }
    public bool IsComplex { get; set; }

    public Rating(IGradable grade, Guid AggregateId, string ItemId, DateTime CreatedAt, string ItemType, string UserId)
        : base(AggregateId, ItemId, CreatedAt, ItemType, UserId)
    {
        RatingId = Guid.NewGuid();
        Grade = grade;
    }

    public void UpdateGrade(IGradable grade)
    {
        this.Grade = grade;
    }

    public float? GetGrade()
    {
        return Grade.getGrade();
    }

    public Guid GetId()
    {
        return RatingId;
    }

    public float? GetMax()
    {
        return Grade.getMax();
    }

    public float? GetMin()
    {
        return Grade.getMin();
    }
}