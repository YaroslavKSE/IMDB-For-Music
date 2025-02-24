using System.Collections.Concurrent;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure;

public class LocalDBTemplate
{
    private static readonly ConcurrentDictionary<Guid, Interaction> Interactions = new();

    public Task<bool> AddReview(string userId, string itemId, string ReviewText)
    {
        Guid interactionId = Guid.NewGuid();
        Review review = new Review(ReviewText, interactionId, itemId, DateTime.Now, "basic");
        Interactions[interactionId] = review;
        return Task.FromResult(true);
    }

    public List<Interaction> GetInteractions()
    {
        List<Interaction> result = new List<Interaction>();
        foreach (var i in Interactions)
        {
            result.Add(i.Value);
        }

        return result;
    }

    public bool IsInteractionsEmpty()
    {
        if (Interactions.Count == 0)
        {
            return true;
        }
        return false;
    }
}