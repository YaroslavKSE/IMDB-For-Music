using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingMethodInstanceEntity
{
    [Key]
    public Guid EntityId { get; set; }

    // Reference to the original grading method template
    public Guid MethodId { get; set; }
    public string Name { get; set; }
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
    public float? Grade { get; set; }
    public float? NormalizedGrade { get; set; }
    public Guid? RatingId { get; set; }

    // Navigation property (optional)
    [ForeignKey("RatingId")]
    public virtual RatingEntity Rating { get; set; }

    // Navigation properties for components and actions
    public virtual ICollection<GradingMethodComponentEntity> Components { get; set; }
    public virtual ICollection<GradingMethodActionEntity> Actions { get; set; }

    public GradingMethodInstanceEntity()
    {
        Components = new List<GradingMethodComponentEntity>();
        Actions = new List<GradingMethodActionEntity>();
    }
}