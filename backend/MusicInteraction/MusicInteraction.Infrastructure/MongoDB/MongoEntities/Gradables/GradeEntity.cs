using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;

[BsonDiscriminator("grade")]
public class GradeEntity : GradableComponentEntity
{
    public GradeEntity()
    {
        ComponentType = "grade";
    }

    public float StepAmount { get; set; }
}