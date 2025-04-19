using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class ReviewCommentEntity
{
    [Key]
    public Guid CommentId { get; set; }
    public Guid ReviewId { get; set; }
    public string UserId { get; set; }
    public DateTime CommentedAt { get; set; }
    public string CommentText { get; set; }

    // Navigation property
    [ForeignKey("ReviewId")]
    public virtual ReviewEntity Review { get; set; }
}