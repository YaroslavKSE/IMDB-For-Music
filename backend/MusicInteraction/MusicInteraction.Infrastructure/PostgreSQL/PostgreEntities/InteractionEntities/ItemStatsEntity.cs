using System.ComponentModel.DataAnnotations;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class ItemStatsEntity
{
    [Key]
    public string ItemId { get; set; }
    public bool IsRaw { get; set; }
    public int TotalUsersInteracted { get; set; }
    public int TotalLikes { get; set; }
    public int TotalReviews { get; set; }
    public int TotalOneRatings { get; set; }
    public int TotalTwoRatings { get; set; }
    public int TotalThreeRatings { get; set; }
    public int TotalFourRatings { get; set; }
    public int TotalFiveRatings { get; set; }
    public int TotalSixRatings { get; set; }
    public int TotalSevenRatings { get; set; }
    public int TotalEightRatings { get; set; }
    public int TotalNineRatings { get; set; }
    public int TotalTenRatings { get; set; }
    public float AverageRating { get; set; }
    public DateTime LastUpdated { get; set; }
}