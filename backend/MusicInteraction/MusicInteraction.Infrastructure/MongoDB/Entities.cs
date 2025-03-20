using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;

// Base entity for MongoDB documents
public abstract class MongoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
}

// MongoDB representation of the GradingMethod
public class GradingMethodEntity : MongoEntity
{
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GradableComponentEntity> Components { get; set; } = new List<GradableComponentEntity>();
    public List<string> Actions { get; set; } = new List<string>();
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}

// Base class for gradable components
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(GradeEntity), typeof(BlockEntity))]
public abstract class GradableComponentEntity
{
    public string Name { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }

    [BsonIgnore]
    public string ComponentType { get; set; }
}

// MongoDB representation of Grade
[BsonDiscriminator("grade")]
public class GradeEntity : GradableComponentEntity
{
    public GradeEntity()
    {
        ComponentType = "grade";
    }

    public float StepAmount { get; set; }
}

// MongoDB representation of GradingBlock
[BsonDiscriminator("block")]
public class BlockEntity : GradableComponentEntity
{
    public BlockEntity()
    {
        ComponentType = "block";
    }

    public List<GradableComponentEntity> Components { get; set; } = new List<GradableComponentEntity>();
    public List<string> Actions { get; set; } = new List<string>();
}