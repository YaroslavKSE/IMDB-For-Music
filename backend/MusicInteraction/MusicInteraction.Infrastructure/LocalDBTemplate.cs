using System.Collections.Concurrent;
using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure;

public class LocalDBTemplate
{
    private static readonly ConcurrentDictionary<Guid, Review> Reviews = new();
    private static readonly ConcurrentDictionary<Guid, Rating> Ratings = new();
    private static readonly ConcurrentDictionary<Guid, Like> Likes = new();
    private static readonly ConcurrentDictionary<Guid, InteractionsAggregate> InteractionsAggregates = new();
    private static readonly ConcurrentDictionary<Guid, GradingMethod> GradingMethods = new();

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

        if (interaction.IsLiked)
        {
            Likes[interaction.AggregateId] = new Like
                (interaction.AggregateId, interaction.ItemId, interaction.CreatedAt, interaction.ItemType, interaction.UserId);
        }

        return;
    }

    public async Task AddGradingMethod(GradingMethod gradingMethod)
    {
        GradingMethods[gradingMethod.SystemId] = gradingMethod;
    }

    public async Task<GradingMethod> GetGradingMethodById(Guid methodId)
    {
        if (GradingMethods.TryGetValue(methodId, out var gradingMethod))
        {
            return gradingMethod;
        }

        throw new KeyNotFoundException($"Grading method with ID {methodId} not found.");
    }

    public async Task<List<GradingMethod>> GetPublicGradingMethods()
    {
        return GradingMethods.Values.Where(gm => gm.IsPublic).ToList();
    }

    public async Task<List<GradingMethod>> GetUserGradingMethods(string userId)
    {
        return GradingMethods.Values.Where(gm => gm.CreatorId == userId).ToList();
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

    public List<Like> GetLikes()
    {
        List<Like> result = new List<Like>();
        foreach (var i in Likes)
        {
            result.Add(i.Value);
        }

        return result;
    }

    public List<Review> GetReviews()
    {
        List<Review> result = new List<Review>();
        foreach (var i in Reviews)
        {
            result.Add(i.Value);
        }

        return result;
    }

    public List<Rating> GetRatings()
    {
        List<Rating> result = new List<Rating>();
        foreach (var i in Ratings)
        {
            result.Add(i.Value);
        }
        return result;
    }

    public bool IsInteractionsEmpty()
    {
        return InteractionsAggregates.Count == 0;
    }

    public bool IsGradingMethodsEmpty()
    {
        return GradingMethods.Count == 0;
    }
}