using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB;

public static class MongoDbSerializationConfig
{
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public static void Initialize()
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            // Configure conventions
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };
            ConventionRegistry.Register("CustomConventions", conventionPack, t => true);

            // Register custom serializers if needed
            BsonSerializer.RegisterSerializer(typeof(Domain.Action), new EnumSerializer<Domain.Action>(BsonType.String));

            // Register class maps for discriminators
            RegisterClassMaps();

            _initialized = true;
        }
    }

    private static void RegisterClassMaps()
    {
        // Only register if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(GradableComponentEntity)))
        {
            BsonClassMap.RegisterClassMap<GradableComponentEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
                cm.SetDiscriminator("gradableComponent");
                cm.SetDiscriminatorIsRequired(true);
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<GradeEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator("grade");
            });

            BsonClassMap.RegisterClassMap<BlockEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator("block");
            });
        }
    }
}