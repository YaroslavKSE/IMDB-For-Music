using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusicInteraction.Infrastructure.MongoDB.Entities;
public abstract class MongoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
}