namespace MusicInteraction.Application.Interfaces;

using MusicInteraction.Domain;

public interface IInteractionStorage
{
    Task<bool> IsEmpty();
    Task AddInteractionAsync(InteractionsAggregate interaction);
    Task<List<InteractionsAggregate>> GetInteractions();
}