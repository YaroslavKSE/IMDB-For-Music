using System.Collections.Concurrent;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure;

public class LocalDBTemplate
{
    private static readonly ConcurrentDictionary<Guid, Interaction> Interactions = new();
    private static readonly ConcurrentDictionary<Guid, Review> Reviews = new();
    private static readonly ConcurrentDictionary<Guid, Rating> Ratings = new();
    private static readonly ConcurrentDictionary<Guid, InteractionsAggregate> InteractionsAggregates = new();

    public async Task AddInteraction(InteractionsAggregate interaction)
    {
        InteractionsAggregates[interaction.AggregateId] = interaction;

        if (interaction.Review != null)
        {
            Reviews[interaction.Review.getReviewId()] = interaction.Review;
        }

        if (interaction.Rating != null)
        {
            Ratings[interaction.Rating.GetId()] = interaction.Rating;
        }

        return;
    }
    
    public Task<bool> AddReview(string userId, string itemId, string ReviewText)
    {
        Guid interactionId = Guid.NewGuid();
        Review review = new Review(ReviewText, interactionId, itemId, DateTime.Now, "basic", userId);
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