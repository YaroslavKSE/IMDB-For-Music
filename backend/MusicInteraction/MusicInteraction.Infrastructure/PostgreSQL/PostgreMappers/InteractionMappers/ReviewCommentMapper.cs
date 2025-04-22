using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class ReviewCommentMapper
{
    public static ReviewCommentEntity ToEntity(ReviewComment domain)
    {
        return new ReviewCommentEntity
        {
            CommentId = domain.CommentId,
            ReviewId = domain.ReviewId,
            UserId = domain.UserId,
            CommentedAt = domain.CommentedAt,
            CommentText = domain.CommentText
        };
    }

    public static ReviewComment ToDomain(ReviewCommentEntity entity)
    {
        return new ReviewComment(
            entity.CommentId,
            entity.ReviewId,
            entity.UserId,
            entity.CommentedAt,
            entity.CommentText
        );
    }
}