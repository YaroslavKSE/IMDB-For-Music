using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;

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