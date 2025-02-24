namespace MusicInteraction.Domain;

public class Rating: Interaction
{
    private IGradable Grade;

    public Rating(float grade, Guid InteractionId, string ItemId, DateTime CreatedAt, string ItemType)
        : base(InteractionId, ItemId, CreatedAt, ItemType)
    {
        Grade = new Grade();
        Grade.updateGrade(grade);
    }

    public void UpdateGrade(float grade)
    {
        Grade.updateGrade(grade);
    }

    public float? GetGrade()
    {
        return Grade.getGrade();
    }

    public float? GetMax()
    {
        return Grade.getMax();
    }
}