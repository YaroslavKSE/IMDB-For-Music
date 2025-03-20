using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class RatingEntity
{
    [Key]
    public Guid RatingId { get; set; }
    public bool IsComplexGrading { get; set; }
    public Guid AggregateId { get; set; }

    // Navigation properties
    [ForeignKey("AggregateId")]
    public virtual InteractionAggregateEntity Interaction { get; set; }

    // One-to-one relationship with GradeEntity
    public virtual GradeEntity SimpleGrade { get; set; }

    // One-to-one relationship with GradingMethodInstanceEntity
    public virtual GradingMethodInstanceEntity ComplexGrade { get; set; }
}