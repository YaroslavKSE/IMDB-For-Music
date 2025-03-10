using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure.LocalStorages;

public class InteractionStorage: IInteractionStorage
{
    private readonly LocalDBTemplate Database;

    public InteractionStorage(LocalDBTemplate _db)
    {
        Database = _db;
    }

    public async Task AddInteractionAsync(InteractionsAggregate interaction)
    {
        await Database.AddInteraction(interaction);
    }

    public async Task<bool> AddReview(string userId, string itemId, string text)
    {
        return await Database.AddReview(userId, itemId, text);
    }

    public async Task<bool> IsEmpty()
    {
        return Database.IsInteractionsEmpty();
    }

    public async Task<List<Interaction>> GetInteractions()
    {
        return Database.GetInteractions();
    }


}