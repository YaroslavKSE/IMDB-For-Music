namespace MusicInteraction.Application;

public class GetItemStatsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ItemStatsDto Stats { get; set; }
}

public class ItemStatsDto
{
    public string ItemId { get; set; }
    public int TotalUsersInteracted { get; set; }
    public int TotalLikes { get; set; }
    public int TotalReviews { get; set; }
    public int[] RatingDistribution { get; set; } = new int[10]; // Indices 0-9 represent ratings 1-10
    public float AverageRating { get; set; }
    public bool HasRatings { get; set; }
}