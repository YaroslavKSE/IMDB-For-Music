using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class ReviewEntity
{
    [Key]
    public Guid ReviewId { get; set; }
    public string ReviewText { get; set; }
    public Guid AggregateId { get; set; }

    // Navigation property for parent interaction
    [ForeignKey("AggregateId")]
    public virtual InteractionAggregateEntity Interaction { get; set; }

    // Navigation properties for likes and comments
    public virtual ICollection<ReviewLikeEntity> Likes { get; set; } = new List<ReviewLikeEntity>();
    public virtual ICollection<ReviewCommentEntity> Comments { get; set; } = new List<ReviewCommentEntity>();
}