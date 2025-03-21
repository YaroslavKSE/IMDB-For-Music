using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class LikeEntity
{
    [Key]
    public Guid LikeId { get; set; }
    public Guid AggregateId { get; set; }

    // Navigation property
    [ForeignKey("AggregateId")]
    public virtual InteractionAggregateEntity Interaction { get; set; }
}