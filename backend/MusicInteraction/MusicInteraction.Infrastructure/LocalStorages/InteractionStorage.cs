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

    public async Task<bool> IsEmpty()
    {
        return Database.IsInteractionsEmpty();
    }

    public async Task<List<InteractionsAggregate>> GetInteractions()
    {
        return Database.GetInteractions();
    }

    public async Task<List<Like>> GetLikes()
    {
        return Database.GetLikes();
    }

    public async Task<List<Review>> GetReviews()
    {
        return Database.GetReviews();
    }

    public async Task<List<Rating>> GetRatings()
    {
        return Database.GetRatings();
    }

}