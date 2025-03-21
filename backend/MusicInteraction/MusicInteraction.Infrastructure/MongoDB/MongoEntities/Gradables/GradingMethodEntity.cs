using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;

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