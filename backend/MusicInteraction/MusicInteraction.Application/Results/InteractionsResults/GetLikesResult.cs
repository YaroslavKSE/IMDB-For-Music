using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetLikesResult
{
    public bool LikesEmpty { get; set; }
    public List<Like> Likes { get; set; }
}