using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingMethodComponentEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid GradingMethodId { get; set; }
    public string ComponentType { get; set; }
    public int ComponentNumber { get; set; }
    public Guid? BlockComponentId { get; set; }
    public Guid? GradeComponentId { get; set; }

    // Navigation properties
    [ForeignKey("GradingMethodId")]
    public virtual GradingMethodInstanceEntity GradingMethod { get; set; }

    [ForeignKey("BlockComponentId")]
    public virtual GradingBlockEntity BlockComponent { get; set; }

    [ForeignKey("GradeComponentId")]
    public virtual GradeEntity GradeComponent { get; set; }
}