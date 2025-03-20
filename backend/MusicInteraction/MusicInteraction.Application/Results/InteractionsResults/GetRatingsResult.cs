namespace MusicInteraction.Application;

public class GetRatingsResult
{
    public bool RatingsEmpty { get; set; }
    public List<RatingOverviewDTO> Ratings { get; set; }
}