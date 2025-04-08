namespace MusicInteraction.Application.Interfaces;

using MusicInteraction.Domain;

public interface IInteractionStorage
{
    Task<bool> IsEmpty();
    Task AddInteractionAsync(InteractionsAggregate interaction);
    Task<List<InteractionsAggregate>> GetInteractions();
    Task<List<InteractionsAggregate>> GetInteractionsByUserId(string userId);
    Task<List<InteractionsAggregate>> GetInteractionsByItemId(string itemId);
    Task<List<InteractionsAggregate>> GetInteractionsByUserAndItem(string userId, string itemId);
    Task<InteractionsAggregate> GetInteractionById(Guid interactionId);
    Task<List<Like>> GetLikes();
    Task<List<Review>> GetReviews();
    Task<List<Rating>> GetRatings();
    Task<Rating> GetRatingById(Guid ratingId);
    Task DeleteInteractionAsync(Guid interactionId);
    Task UpdateInteractionAsync(InteractionsAggregate interaction);
    Task<bool> GetGradingTypeByInteractionId(Guid interactionId);
}