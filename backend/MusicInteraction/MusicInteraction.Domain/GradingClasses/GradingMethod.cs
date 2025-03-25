namespace MusicInteraction.Domain;

public class GradingMethod : GradingBlock
{
    public Guid SystemId { get; private set; }
    public string CreatorId { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Name { get; private set; }

    public GradingMethod(string name, string creatorId, bool isPublic = false)
        : base(name)
    {
        SystemId = Guid.NewGuid();
        CreatorId = creatorId;
        IsPublic = isPublic;
        CreatedAt = DateTime.UtcNow;
        Name = name;
    }

    public GradingMethod(Guid systemId, DateTime createdAt , string name, string creatorId, bool isPublic = false)
        : base(name)
    {
        SystemId = systemId;
        CreatorId = creatorId;
        IsPublic = isPublic;
        CreatedAt = createdAt;
        Name = name;
    }

    public void MakePublic()
    {
        IsPublic = true;
    }

    public void MakePrivate()
    {
        IsPublic = false;
    }
}