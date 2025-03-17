using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure.Migration;

public class GradingMethodMigrator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GradingMethodMigrator> _logger;

    public GradingMethodMigrator(
        IServiceProvider serviceProvider,
        ILogger<GradingMethodMigrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GradingMethod migration service started - Local storage has been removed");

        using var scope = _serviceProvider.CreateScope();
        var mongoStorage = scope.ServiceProvider.GetRequiredService<IGradingMethodStorage>();

        // Check if MongoDB collection is empty
        if (await mongoStorage.IsEmpty())
        {
            _logger.LogInformation("MongoDB collection is empty - adding default grading methods");

            // Add default grading methods if needed
            await CreateDefaultGradingMethods(mongoStorage);
        }
        else
        {
            _logger.LogInformation("MongoDB collection already has data, no migration needed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateDefaultGradingMethods(IGradingMethodStorage storage)
    {
        try
        {
            // Create a default Album Rating System
            var albumRatingSystem = new GradingMethod("Default Rating System", "system", true);

            // Add Lyrics component
            var lyricsGrade = new Grade(1, 10, 0.5f, "Lyrics");
            albumRatingSystem.AddGrade(lyricsGrade);

            // Add Production block with subcomponents
            var productionBlock = new GradingBlock("Production");
            var mixingGrade = new Grade(1, 10, 0.5f, "Mixing");
            var masteringGrade = new Grade(1, 10, 0.5f, "Mastering");

            productionBlock.AddGrade(mixingGrade);
            productionBlock.AddAction(Domain.Action.Add);
            productionBlock.AddGrade(masteringGrade);

            // Add the production block to the main system
            albumRatingSystem.AddAction(Domain.Action.Add);
            albumRatingSystem.AddGrade(productionBlock);

            // Save to storage
            await storage.AddGradingMethodAsync(albumRatingSystem);
            _logger.LogInformation("Added default Album Rating System with ID: {Id}", albumRatingSystem.SystemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default grading methods");
        }
    }
}

// Extension method to register the migrator
public static class MigrationExtensions
{
    public static IServiceCollection AddGradingMethodMigration(this IServiceCollection services)
    {
        services.AddHostedService<GradingMethodMigrator>();
        return services;
    }
}