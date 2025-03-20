using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;
public static class ReviewMapper
{
    public static ReviewEntity ToEntity(Review domain)
    {
        return new ReviewEntity
        {
            ReviewId = domain.ReviewId,
            ReviewText = domain.ReviewText,
            AggregateId = domain.AggregateId,
        };
    }

    public static async Task<Review> ToDomain(ReviewEntity entity, MusicInteractionDbContext dbContext)
    {
        if (entity.Interaction == null)
        {
            entity.Interaction = await dbContext.Interactions
                .FirstOrDefaultAsync(i => i.AggregateId == entity.AggregateId);
        }

        var review = new Review(
            entity.ReviewText,
            entity.AggregateId,
            entity.Interaction.ItemId,
            entity.Interaction.CreatedAt,
            entity.Interaction.ItemType,
            entity.Interaction.UserId
        );
        review.ReviewId = entity.ReviewId;
        return review;
    }
}