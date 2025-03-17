using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        // Initialize MongoDB serialization configuration
        MongoDbSerializationConfig.Initialize();

        // Get connection string from configuration
        var connectionString = configuration["MongoDB:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "mongodb+srv://mpozdniakov:kElQkUOLQzhRHtzl@gradingmethods.dm47r.mongodb.net/?retryWrites=true&w=majority&appName=GradingMethods";
        }

        // Get database name from configuration or use default
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "MusicEvaluationPlatform";

        // Create the MongoDB client
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    // Collection for GradingMethods
    public IMongoCollection<GradingMethodEntity> GradingMethods =>
        _database.GetCollection<GradingMethodEntity>("GradingMethods");
}