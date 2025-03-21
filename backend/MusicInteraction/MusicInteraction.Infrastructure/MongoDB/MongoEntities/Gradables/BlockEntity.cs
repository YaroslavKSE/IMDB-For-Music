using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;

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