using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeComponentDto), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(BlockComponentDto), typeDiscriminator: "block")]
public abstract class ComponentDto
{
    public string Name { get; set; }
}