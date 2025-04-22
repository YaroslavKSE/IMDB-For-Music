using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class ReviewLikeMapper
{
    public static ReviewLikeEntity ToEntity(ReviewLike domain)
    {
        return new ReviewLikeEntity
        {
            LikeId = domain.LikeId,
            ReviewId = domain.ReviewId,
            UserId = domain.UserId,
            LikedAt = domain.LikedAt
        };
    }

    public static ReviewLike ToDomain(ReviewLikeEntity entity)
    {
        return new ReviewLike(
            entity.LikeId,
            entity.ReviewId,
            entity.UserId,
            entity.LikedAt
        );
    }
}