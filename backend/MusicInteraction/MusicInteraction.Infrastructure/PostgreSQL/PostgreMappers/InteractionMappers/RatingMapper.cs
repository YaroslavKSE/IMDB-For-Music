using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

/// <summary>
/// Mapper for converting between Rating domain models and database entities
/// </summary>
public static class RatingMapper
{
    /// <summary>
    /// Maps a Rating domain model to its corresponding database entities
    /// </summary>
    public static async Task<RatingEntity> ToEntityAsync(Rating domain, MusicInteractionDbContext dbContext)
    {
        var ratingEntity = new RatingEntity
        {
            RatingId = domain.RatingId,
            AggregateId = domain.AggregateId,
            IsComplexGrading = domain.Grade is not Grade
        };

        await dbContext.Ratings.AddAsync(ratingEntity);

        // Handle different types of gradable components
        if (domain.Grade is Grade simpleGrade)
        {
            await GradeMapper.MapToEntityAsync(simpleGrade, dbContext, ratingEntity.RatingId);
        }
        else if (domain.Grade is GradingMethod gradingMethod)
        {
            await GradingMethodMapper.MapToEntityAsync(gradingMethod, dbContext, ratingEntity.RatingId);
        }

        return ratingEntity;
    }

    /// <summary>
    /// Maps a RatingEntity database entity to its corresponding Rating domain model
    /// </summary>
    public static async Task<Rating> ToDomainAsync(RatingEntity entity, MusicInteractionDbContext dbContext)
    {
        // First, reconstruct the gradable component
        IGradable gradable;

        if (!entity.IsComplexGrading)
        {
            gradable = await GradeMapper.GetByRatingIdAsync(entity.RatingId, dbContext);
        }
        else
        {
            gradable = await GradingMethodMapper.GetByRatingIdAsync(entity.RatingId, dbContext);
        }

        if (entity.Interaction == null)
        {
            entity.Interaction = await dbContext.Interactions
                .FirstOrDefaultAsync(i => i.AggregateId == entity.AggregateId);
        }

        // Create the rating with the reconstructed gradable
        var rating = new Rating(
            gradable,
            entity.AggregateId,
            entity.Interaction.ItemId,
            entity.Interaction.CreatedAt,
            entity.Interaction.ItemType,
            entity.Interaction.UserId
        );

        rating.RatingId = entity.RatingId;
        return rating;
    }
}