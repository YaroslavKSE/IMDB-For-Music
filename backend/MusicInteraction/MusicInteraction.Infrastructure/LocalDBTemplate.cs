using System.Collections.Concurrent;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure;

public class LocalDBTemplate
{
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

    public List<InteractionsAggregate> GetInteractions()
    {
        List<InteractionsAggregate> result = new List<InteractionsAggregate>();
        foreach (var i in InteractionsAggregates)
        {
            result.Add(i.Value);
        }

        return result;
    }

    public bool IsInteractionsEmpty()
    {
        if (InteractionsAggregates.Count == 0)
        {
            return true;
        }
        return false;
    }
}