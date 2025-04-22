using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Infrastructure.Configuration;
using MusicCatalogService.Migrations.Services;

namespace MusicCatalogService.Migrations;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting MongoDB migration to optimize storage...");

        // Create a host with the same configuration as your main app
        using var host = CreateHostBuilder(args).Build();

        // Register MongoDB serializers for special types
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        // Register class maps for MongoDB
        RegisterMongoClassMaps();

        // Get the migration services from DI
        var migrationService = host.Services.GetRequiredService<MongoDbMigrationService>();
        var indexService = host.Services.GetRequiredService<MongoDbIndexCreationService>();

        try
        {
            // First ensure indexes exist
            Console.WriteLine("Creating and updating indexes...");
            await indexService.CreateIndexesAsync();

            // Then run the data migration
            Console.WriteLine("Starting data migration...");
            await migrationService.MigrateAsync();
            
            // Validate migration
            Console.WriteLine("Validating migrations...!");
            await migrationService.ValidateMigrationAsync();
            
            // Print the collection status 
            Console.WriteLine("Gathering the logs...");
            await migrationService.LogCollectionSizes();

            

            Console.WriteLine("Migration completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => 
            {
                // Use environment variables as primary configuration source
                config.AddEnvironmentVariables("MongoDb__");
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Configure MongoDB settings
                services.Configure<MongoDbSettings>(
                    hostContext.Configuration.GetSection("MongoDb"));

                // Register migration services
                services.AddSingleton<MongoDbMigrationService>();
                services.AddSingleton<MongoDbIndexCreationService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
    }

    private static void RegisterMongoClassMaps()
    {
        // Register class maps to ensure proper serialization
        if (!BsonClassMap.IsClassMapRegistered(typeof(CatalogItemBase)))
            BsonClassMap.RegisterClassMap<CatalogItemBase>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(Album)))
            BsonClassMap.RegisterClassMap<Album>();

        if (!BsonClassMap.IsClassMapRegistered(typeof(Track)))
            BsonClassMap.RegisterClassMap<Track>();

        if (!BsonClassMap.IsClassMapRegistered(typeof(Artist)))
            BsonClassMap.RegisterClassMap<Artist>();

        if (!BsonClassMap.IsClassMapRegistered(typeof(SimplifiedArtist)))
            BsonClassMap.RegisterClassMap<SimplifiedArtist>();
    }
}