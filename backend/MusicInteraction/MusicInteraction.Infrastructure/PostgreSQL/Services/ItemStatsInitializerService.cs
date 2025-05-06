using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Infrastructure.PostgreSQL;

namespace MusicInteraction.Infrastructure.Services;

/// <summary>
/// Service responsible for initializing ItemStats records for all items that have interactions
/// Runs once on application startup
/// </summary>
public class ItemStatsInitializerService : IHostedService
{
    private readonly ILogger<ItemStatsInitializerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ItemStatsInitializerService(
        ILogger<ItemStatsInitializerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemStatsInitializerService starting...");
        await InitializeItemStats(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemStatsInitializerService stopped");
        return Task.CompletedTask;
    }

    private async Task InitializeItemStats(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MusicInteractionDbContext>();
            var itemStatsStorage = scope.ServiceProvider.GetRequiredService<IItemStatsStorage>();

            _logger.LogInformation("Finding items without ItemStats records...");

            // Get all unique item IDs from interactions
            var allItemIds = await dbContext.Interactions
                .Select(i => i.ItemId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Get existing item stats
            var existingItemIds = await dbContext.ItemStats
                .Select(s => s.ItemId)
                .ToListAsync(cancellationToken);

            // Find items without stats
            var itemIdsWithoutStats = allItemIds
                .Except(existingItemIds)
                .ToList();

            if (itemIdsWithoutStats.Count == 0)
            {
                _logger.LogInformation("All items already have ItemStats records. No initialization needed.");
                return;
            }

            _logger.LogInformation("Found {Count} items without ItemStats records. Initializing...", itemIdsWithoutStats.Count);

            // Initialize stats for each item
            int counter = 0;
            foreach (var itemId in itemIdsWithoutStats)
            {
                await itemStatsStorage.InitializeItemStatsAsync(itemId);
                counter++;

                if (counter % 100 == 0 || counter == itemIdsWithoutStats.Count)
                {
                    _logger.LogInformation("Initialized ItemStats for {Counter}/{Total} items", counter, itemIdsWithoutStats.Count);
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("ItemStats initialization was cancelled after processing {Counter}/{Total} items", counter, itemIdsWithoutStats.Count);
                    break;
                }
            }

            _logger.LogInformation("ItemStats initialization completed. Created {Count} new ItemStats records", counter);

            // Process the newly created raw item stats
            _logger.LogInformation("Processing raw item stats...");
            await itemStatsStorage.ProcessAllRawItemStatsAsync();
            _logger.LogInformation("Completed processing raw item stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ItemStats records");
        }
    }
}