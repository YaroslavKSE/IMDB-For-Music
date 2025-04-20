using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeDetailDTO), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(GradedBlockDetailDTO), typeDiscriminator: "block")]
public abstract class GradedComponentDTO
{
    public string Name { get; set; }
    public float? CurrentGrade { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}