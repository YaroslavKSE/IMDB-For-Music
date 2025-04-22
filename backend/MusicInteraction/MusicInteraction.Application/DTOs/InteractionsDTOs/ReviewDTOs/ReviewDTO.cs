namespace MusicInteraction.Application;

public class ReviewDTO
{
    public Guid ReviewId { get; set; }
    public string ReviewText { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
}