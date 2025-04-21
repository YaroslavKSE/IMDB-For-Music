using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Infrastructure.Services;
using MusicLists.Infrastructure.DBConfig;

namespace MusicLists.Infrastructure.Extensions;

public class ListHotScoreUpdateService : BackgroundService
{
    private readonly ILogger<ListHotScoreUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _updateInterval = TimeSpan.FromDays(1);

    public ListHotScoreUpdateService(
        ILogger<ListHotScoreUpdateService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Use a timer to run the task every day
        using var timer = new PeriodicTimer(_updateInterval);

        // Run immediately on startup
        await UpdateHotScores(stoppingToken);

        // Then run at the specified interval
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateHotScores(stoppingToken);
        }
    }

    private async Task UpdateHotScores(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to update lists hot scores");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MusicListsDbContext>();
            var calculator = new ListHotScoreCalculator();

            // Get reviews that are either dirty or less than 31 days old
            var cutoffDate = DateTime.UtcNow.AddDays(-31);
            var lists = await dbContext.Lists
                .Include(r => r.Likes)
                .Include(r => r.Comments)
                .Where(r => r.IsScoreDirty || r.CreatedAt > cutoffDate)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {ReviewCount} lists to update", lists.Count);

            foreach (var list in lists)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                int likesCount = list.Likes.Count;
                int commentsCount = list.Comments.Count;
                DateTime createdAt = list.CreatedAt;

                // Calculate the new hot score
                float newHotScore = calculator.CalculateHotScore(likesCount, commentsCount, createdAt);

                // Update the list entity
                list.HotScore = newHotScore;
                list.IsScoreDirty = false;
            }

            // Save all changes
            await dbContext.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Successfully updated hot scores for {ReviewCount} reviews", lists.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review hot scores");
        }
    }
}