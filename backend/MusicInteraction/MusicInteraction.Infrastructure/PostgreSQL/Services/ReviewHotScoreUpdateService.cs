using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Infrastructure.PostgreSQL;

namespace MusicInteraction.Infrastructure.Services;

public class ReviewHotScoreUpdateService : BackgroundService
{
    private readonly ILogger<ReviewHotScoreUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _updateInterval = TimeSpan.FromDays(1);

    public ReviewHotScoreUpdateService(
        ILogger<ReviewHotScoreUpdateService> logger,
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
        _logger.LogInformation("Starting to update review hot scores");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MusicInteractionDbContext>();
            var calculator = new ReviewHotScoreCalculator();

            // Get reviews that are either dirty or less than 31 days old
            var cutoffDate = DateTime.UtcNow.AddDays(-31);
            var reviews = await dbContext.Reviews
                .Include(r => r.Interaction)
                .Include(r => r.Likes)
                .Include(r => r.Comments)
                .Where(r => r.IsScoreDirty || r.Interaction.CreatedAt > cutoffDate)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {ReviewCount} reviews to update", reviews.Count);

            foreach (var review in reviews)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                int likesCount = review.Likes.Count;
                int commentsCount = review.Comments.Count;
                DateTime createdAt = review.Interaction.CreatedAt;

                // Calculate the new hot score
                float newHotScore = calculator.CalculateHotScore(likesCount, commentsCount, createdAt);

                // Update the review entity
                review.HotScore = newHotScore;
                review.IsScoreDirty = false;
            }

            // Save all changes
            await dbContext.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Successfully updated hot scores for {ReviewCount} reviews", reviews.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review hot scores");
        }
    }
}