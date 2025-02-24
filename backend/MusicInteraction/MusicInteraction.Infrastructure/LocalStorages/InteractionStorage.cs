using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Infrastructure.LocalStorages;

public class InteractionStorage: IInteractionStorage
{
    private readonly LocalDBTemplate Database;

    public InteractionStorage(LocalDBTemplate _db)
    {
        Database = _db;
    }

    public async Task<bool> AddReview(string userId, string itemId, string text)
    {
        return await Database.AddReview(userId, itemId, text);
    }
}