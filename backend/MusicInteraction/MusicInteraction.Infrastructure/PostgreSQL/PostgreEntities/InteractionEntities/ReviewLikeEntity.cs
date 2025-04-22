using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class ReviewLikeEntity
{
    [Key]
    public Guid LikeId { get; set; }
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
    public DateTime LikedAt { get; set; }

    // Navigation property
    [ForeignKey("ReviewId")]
    public virtual ReviewEntity Review { get; set; }
}