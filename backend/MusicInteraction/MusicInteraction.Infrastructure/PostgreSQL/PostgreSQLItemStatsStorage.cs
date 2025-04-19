using Microsoft.EntityFrameworkCore;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL;

public class PostgreSQLItemStatsStorage : IItemStatsStorage
{
    private readonly MusicInteractionDbContext _dbContext;

    public PostgreSQLItemStatsStorage(MusicInteractionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ItemStats> GetItemStatsAsync(string itemId)
    {
        var statsEntity = await _dbContext.ItemStats
            .FirstOrDefaultAsync(s => s.ItemId == itemId);

        if (statsEntity == null)
        {
            return new ItemStats { ItemId = itemId };
        }

        return MapToDomain(statsEntity);
    }

    public async Task UpdateItemStatsAsync(string itemId)
    {
        // Get all interactions for this item
        var interactions = await _dbContext.Interactions
            .Where(i => i.ItemId == itemId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        if (interactions.Count == 0)
        {
            return;
        }

        // Get or create stats entity
        var statsEntity = await _dbContext.ItemStats.FindAsync(itemId);
        if (statsEntity == null)
        {
            statsEntity = new ItemStatsEntity
            {
                ItemId = itemId,
                IsRaw = false,
                LastUpdated = DateTime.UtcNow
            };
            await _dbContext.ItemStats.AddAsync(statsEntity);
        }

        // Count unique users
        var uniqueUsers = interactions
            .Select(i => i.UserId)
            .Distinct()
            .Count();

        // Count likes (only latest interaction per user)
        var userLatestInteractions = interactions
            .GroupBy(i => i.UserId)
            .Select(g => g.OrderByDescending(i => i.CreatedAt).First())
            .ToList();

        var totalLikes = userLatestInteractions.Count(i => _dbContext.Likes.Any(l => l.AggregateId == i.AggregateId));

        // Count reviews (only latest per user)
        var totalReviews = userLatestInteractions.Count(i => _dbContext.Reviews.Any(r => r.AggregateId == i.AggregateId));

        // Get all ratings (only latest per user)
        var ratingIds = userLatestInteractions
            .Where(i => _dbContext.Ratings.Any(r => r.AggregateId == i.AggregateId))
            .Select(i => i.AggregateId)
            .ToList();

        var ratings = await _dbContext.Ratings
            .Where(r => ratingIds.Contains(r.AggregateId))
            .ToListAsync();

        // Count ratings by value (normalized to 1-10 scale)
        int[] ratingCounts = new int[10];

        foreach (var rating in ratings)
        {
            float? normalizedValue = null;

            if (!rating.IsComplexGrading)
            {
                // Simple rating
                var grade = await _dbContext.Grades
                    .FirstOrDefaultAsync(g => g.RatingId == rating.RatingId);

                normalizedValue = grade?.NormalizedGrade;
            }
            else
            {
                // Complex rating
                var complexGrade = await _dbContext.GradingMethodInstances
                    .FirstOrDefaultAsync(g => g.RatingId == rating.RatingId);

                normalizedValue = complexGrade?.NormalizedGrade;
            }

            if (normalizedValue.HasValue)
            {
                int index = (int)Math.Round(normalizedValue.Value) - 1;
                if (index >= 0 && index < 10)
                {
                    ratingCounts[index]++;
                }
            }
        }

        // Calculate average rating
        float averageRating = 0;
        int totalRatings = ratings.Count;
        if (totalRatings > 0)
        {
            float sum = 0;
            int count = 0;

            for (int i = 0; i < 10; i++)
            {
                sum += (i + 1) * ratingCounts[i];
                count += ratingCounts[i];
            }

            if (count > 0)
            {
                averageRating = sum / count;
            }
        }

        // Update stats entity
        statsEntity.TotalUsersInteracted = uniqueUsers;
        statsEntity.TotalLikes = totalLikes;
        statsEntity.TotalReviews = totalReviews;
        statsEntity.TotalOneRatings = ratingCounts[0];
        statsEntity.TotalTwoRatings = ratingCounts[1];
        statsEntity.TotalThreeRatings = ratingCounts[2];
        statsEntity.TotalFourRatings = ratingCounts[3];
        statsEntity.TotalFiveRatings = ratingCounts[4];
        statsEntity.TotalSixRatings = ratingCounts[5];
        statsEntity.TotalSevenRatings = ratingCounts[6];
        statsEntity.TotalEightRatings = ratingCounts[7];
        statsEntity.TotalNineRatings = ratingCounts[8];
        statsEntity.TotalTenRatings = ratingCounts[9];
        statsEntity.AverageRating = averageRating;
        statsEntity.IsRaw = false;
        statsEntity.LastUpdated = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkItemStatsAsRawAsync(string itemId)
    {
        var statsEntity = await _dbContext.ItemStats.FindAsync(itemId);
        if (statsEntity != null)
        {
            statsEntity.IsRaw = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task ProcessAllRawItemStatsAsync()
    {
        var rawItemIds = await _dbContext.ItemStats
            .Where(s => s.IsRaw)
            .Select(s => s.ItemId)
            .ToListAsync();

        foreach (var itemId in rawItemIds)
        {
            await UpdateItemStatsAsync(itemId);
        }
    }

    public async Task<bool> ItemStatsExistsAsync(string itemId)
    {
        return await _dbContext.ItemStats.AnyAsync(s => s.ItemId == itemId);
    }

    public async Task InitializeItemStatsAsync(string itemId)
    {
        bool exists = await ItemStatsExistsAsync(itemId);
        if (!exists)
        {
            var statsEntity = new ItemStatsEntity
            {
                ItemId = itemId,
                IsRaw = true,
                LastUpdated = DateTime.UtcNow
            };
            await _dbContext.ItemStats.AddAsync(statsEntity);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            await MarkItemStatsAsRawAsync(itemId);
        }
    }

    private ItemStats MapToDomain(ItemStatsEntity entity)
    {
        return new ItemStats
        {
            ItemId = entity.ItemId,
            IsRaw = entity.IsRaw,
            TotalUsersInteracted = entity.TotalUsersInteracted,
            TotalLikes = entity.TotalLikes,
            TotalReviews = entity.TotalReviews,
            TotalOneRatings = entity.TotalOneRatings,
            TotalTwoRatings = entity.TotalTwoRatings,
            TotalThreeRatings = entity.TotalThreeRatings,
            TotalFourRatings = entity.TotalFourRatings,
            TotalFiveRatings = entity.TotalFiveRatings,
            TotalSixRatings = entity.TotalSixRatings,
            TotalSevenRatings = entity.TotalSevenRatings,
            TotalEightRatings = entity.TotalEightRatings,
            TotalNineRatings = entity.TotalNineRatings,
            TotalTenRatings = entity.TotalTenRatings,
            Rating = entity.AverageRating
        };
    }
}