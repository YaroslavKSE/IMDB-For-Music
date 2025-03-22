using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class InteractionMapper
{
    public static InteractionAggregateEntity ToEntity(InteractionsAggregate domain)
    {
        var entity = new InteractionAggregateEntity
        {
            AggregateId = domain.AggregateId,
            UserId = domain.UserId,
            ItemId = domain.ItemId,
            ItemType = domain.ItemType,
            CreatedAt = domain.CreatedAt,
        };

        return entity;
    }

    public static async Task<InteractionsAggregate> ToDomain(InteractionAggregateEntity entity,
        MusicInteractionDbContext dbContext)
    {
        var domain = new InteractionsAggregate(entity.UserId, entity.ItemId, entity.ItemType);

        // Use reflection to set the private fields that can't be set through constructor
        typeof(InteractionsAggregate).GetProperty("AggregateId")?.SetValue(domain, entity.AggregateId);
        typeof(InteractionsAggregate).GetProperty("CreatedAt")?.SetValue(domain, entity.CreatedAt);

        // Check if there's a like and set the IsLiked property accordingly
        var likeEntity = await dbContext.Likes.FirstOrDefaultAsync(l => l.AggregateId == entity.AggregateId);
        if (likeEntity != null)
        {
            typeof(InteractionsAggregate).GetProperty("IsLiked")?.SetValue(domain, true);
        }
        else
        {
            typeof(InteractionsAggregate).GetProperty("IsLiked")?.SetValue(domain, false);
        }

        // Load review if exists - use the navigation property or query by AggregateId
        var reviewEntity = await dbContext.Reviews.FirstOrDefaultAsync(r => r.AggregateId == entity.AggregateId);
        if (reviewEntity != null)
        {
            typeof(InteractionsAggregate).GetProperty("Review")?.SetValue(
                domain,
                ReviewMapper.ToDomain(reviewEntity, dbContext).Result
            );
        }

        try {
            // Load rating if exists - use the navigation property or query by AggregateId
            var ratingEntity = await dbContext.Ratings.FirstOrDefaultAsync(r => r.AggregateId == entity.AggregateId);
            if (ratingEntity != null)
            {
                typeof(InteractionsAggregate).GetProperty("Rating")?.SetValue(
                    domain,
                    await RatingMapper.ToDomainAsync(ratingEntity, dbContext)
                );
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error loading rating for interaction {entity.AggregateId}: {ex.Message}");
        }

        return domain;
    }
}