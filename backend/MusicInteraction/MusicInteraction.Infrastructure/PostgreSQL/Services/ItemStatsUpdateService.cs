using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Infrastructure.Services;

public class ItemStatsUpdateService : BackgroundService
{
    private readonly ILogger<ItemStatsUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(60);

    public ItemStatsUpdateService(
        ILogger<ItemStatsUpdateService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        // Run immediately on startup
        await ProcessRawItemStats(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessRawItemStats(stoppingToken);
        }
    }

    private async Task ProcessRawItemStats(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ItemStatsUpdateService is updating raw item stats");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var itemStatsStorage = scope.ServiceProvider.GetRequiredService<IItemStatsStorage>();

            await itemStatsStorage.ProcessAllRawItemStatsAsync();

            _logger.LogInformation("ItemStatsUpdateService has completed updating raw item stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating raw item stats");
        }
    }
}