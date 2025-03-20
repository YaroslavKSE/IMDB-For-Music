using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingBlockEntity
{
    [Key]
    public Guid EntityId { get; set; }
    public string Name { get; set; }
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
    public float? Grade { get; set; }
    public float? NormalizedGrade { get; set; }

    // Navigation properties for components and actions
    public virtual ICollection<GradingBlockComponentEntity> Components { get; set; }
    public virtual ICollection<GradingBlockActionEntity> Actions { get; set; }

    // Navigation for reverse relationships
    public virtual ICollection<GradingMethodComponentEntity> MethodComponents { get; set; }
    public virtual ICollection<GradingBlockComponentEntity> ParentBlockComponents { get; set; }

    public GradingBlockEntity()
    {
        Components = new List<GradingBlockComponentEntity>();
        Actions = new List<GradingBlockActionEntity>();
        MethodComponents = new List<GradingMethodComponentEntity>();
        ParentBlockComponents = new List<GradingBlockComponentEntity>();
    }
}