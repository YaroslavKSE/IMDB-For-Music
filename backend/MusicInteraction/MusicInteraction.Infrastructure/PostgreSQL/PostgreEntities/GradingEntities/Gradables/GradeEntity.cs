using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradeEntity
{
    [Key]
    public Guid EntityId { get; set; }
    public string Name { get; set; }
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
    public float? Grade { get; set; }
    public float StepAmount { get; set; }
    public float? NormalizedGrade { get; set; }

    // This field can be null when the grade is part of a grading block
    public Guid? RatingId { get; set; }

    // Navigation property (optional)
    [ForeignKey("RatingId")]
    public virtual RatingEntity Rating { get; set; }

    // Navigation for reverse relationships
    public virtual ICollection<GradingMethodComponentEntity> MethodComponents { get; set; }
    public virtual ICollection<GradingBlockComponentEntity> BlockComponents { get; set; }
}