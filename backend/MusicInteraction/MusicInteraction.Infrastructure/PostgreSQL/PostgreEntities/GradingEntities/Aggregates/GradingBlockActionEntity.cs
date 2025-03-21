using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingBlockActionEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid GradingBlockId { get; set; }
    public int ActionNumber { get; set; }
    public string ActionType { get; set; }

    // Navigation property
    [ForeignKey("GradingBlockId")]
    public virtual GradingBlockEntity GradingBlock { get; set; }
}