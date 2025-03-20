using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingBlockComponentEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid GradingBlockId { get; set; }
    public string ComponentType { get; set; }
    public int ComponentNumber { get; set; }
    public Guid? BlockComponentId { get; set; }
    public Guid? GradeComponentId { get; set; }

    // Navigation properties
    [ForeignKey("GradingBlockId")]
    public virtual GradingBlockEntity GradingBlock { get; set; }

    [ForeignKey("BlockComponentId")]
    public virtual GradingBlockEntity BlockComponent { get; set; }

    [ForeignKey("GradeComponentId")]
    public virtual GradeEntity GradeComponent { get; set; }
}