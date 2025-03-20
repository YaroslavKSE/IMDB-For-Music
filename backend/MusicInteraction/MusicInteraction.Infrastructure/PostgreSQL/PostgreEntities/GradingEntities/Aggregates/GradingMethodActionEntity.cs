using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities;

public class GradingMethodActionEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid GradingMethodId { get; set; }
    public int ActionNumber { get; set; }
    public string ActionType { get; set; }

    // Navigation property
    [ForeignKey("GradingMethodId")]
    public virtual GradingMethodInstanceEntity GradingMethod { get; set; }
}