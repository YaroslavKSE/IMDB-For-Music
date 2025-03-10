namespace MusicInteraction.Application.Interfaces;

using MusicInteraction.Domain;

public interface IInteractionStorage
{
    Task<bool> AddReview(string userId, string itemId, string reviewText);
    Task<bool> IsEmpty();
    Task AddInteractionAsync(InteractionsAggregate interaction);
    Task<List<Interaction>> GetInteractions();
}