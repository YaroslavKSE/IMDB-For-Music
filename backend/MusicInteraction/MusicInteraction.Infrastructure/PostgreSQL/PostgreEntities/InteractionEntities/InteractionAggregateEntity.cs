using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class InteractionAggregateEntity
{
    [Key]
    public Guid AggregateId { get; set; }
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual RatingEntity Rating { get; set; }
    public virtual ReviewEntity Review { get; set; }
    public virtual LikeEntity Like { get; set; }
}