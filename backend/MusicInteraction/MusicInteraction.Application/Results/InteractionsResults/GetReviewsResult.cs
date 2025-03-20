using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetReviewsResult
{
    public bool ReviewsEmpty { get; set; }
    public List<Review> Reviews { get; set; }
}