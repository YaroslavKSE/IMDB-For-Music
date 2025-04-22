namespace MusicInteraction.Application.Interfaces;

using Domain;

public interface IInteractionStorage
{
    Task<bool> IsEmpty();
    Task AddInteractionAsync(InteractionsAggregate interaction);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractions(int? limit = null, int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractionsByUserId(string userId, int? limit = null, int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractionsByItemId(string itemId, int? limit = null, int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetReviewedInteractionsByItemId(string itemId, bool? useHotScore = true, int? limit = null,
        int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractionsByUserAndItem(string userId, string itemId, int? limit = null, int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractionsByUserIds(List<string> userIds, int? limit = null, int? offset = null);
    Task<PaginatedResult<InteractionsAggregate>> GetInteractionsByItemIds(List<string> itemIds, int? limit = null, int? offset = null);
    Task<InteractionsAggregate> GetInteractionById(Guid interactionId);
    Task<List<Like>> GetLikes();
    Task<List<Review>> GetReviews();
    Task<List<Rating>> GetRatings();
    Task<Rating> GetRatingById(Guid ratingId);
    Task DeleteInteractionAsync(Guid interactionId);
    Task UpdateInteractionAsync(InteractionsAggregate interaction);
    // Review Interactions
    Task<ReviewLike> AddReviewLike(Guid reviewId, string userId);
    Task<bool> RemoveReviewLike(Guid reviewId, string userId);
    Task<bool> HasUserLikedReview(Guid reviewId, string userId);
    Task<ReviewComment> AddReviewComment(Guid reviewId, string userId, string commentText);
    Task<bool> DeleteReviewComment(Guid commentId, string userId);
    Task<PaginatedResult<ReviewComment>> GetReviewComments(Guid reviewId, int? limit = null, int? offset = null);
}

public class PaginatedResult<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }

    public PaginatedResult(List<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}