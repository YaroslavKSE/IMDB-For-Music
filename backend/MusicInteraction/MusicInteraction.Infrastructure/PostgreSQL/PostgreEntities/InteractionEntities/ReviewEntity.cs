using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class ReviewEntity
{
    [Key]
    public Guid ReviewId { get; set; }
    public string ReviewText { get; set; }
    public Guid AggregateId { get; set; }

    // Navigation property
    [ForeignKey("AggregateId")]
    public virtual InteractionAggregateEntity Interaction { get; set; }
}