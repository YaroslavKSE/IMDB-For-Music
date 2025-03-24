namespace MusicInteraction.Application.Interfaces;

using MusicInteraction.Domain;

public interface IInteractionStorage
{
    Task<bool> IsEmpty();
    Task AddInteractionAsync(InteractionsAggregate interaction);
    Task<List<InteractionsAggregate>> GetInteractions();
    Task<InteractionsAggregate> GetInteractionById(Guid interactionId);
    Task<List<Like>> GetLikes();
    Task<List<Review>> GetReviews();
    Task<List<Rating>> GetRatings();
    Task<Rating> GetRatingById(Guid ratingId);
    Task DeleteInteractionAsync(Guid interactionId);
    Task UpdateInteractionAsync(InteractionsAggregate interaction);
}