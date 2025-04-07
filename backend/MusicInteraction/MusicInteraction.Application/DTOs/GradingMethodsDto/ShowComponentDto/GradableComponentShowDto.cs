using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeDetailShowDto), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(BlockDetailShowDto), typeDiscriminator: "block")]
public abstract class GradableComponentShowDto
{
    public string Name { get; set; }
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
}