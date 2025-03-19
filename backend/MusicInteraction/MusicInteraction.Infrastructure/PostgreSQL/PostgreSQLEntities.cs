using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MusicInteraction.Infrastructure.PostgreSQL.Entities
{
    public class InteractionAggregateEntity
    {
        [Key]
        public Guid AggregateId { get; set; }
        public string UserId { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLiked { get; set; }

        // Navigation properties
        public virtual RatingEntity Rating { get; set; }
        public virtual ReviewEntity Review { get; set; }
    }

    public class ReviewEntity
    {
        [Key]
        public Guid ReviewId { get; set; }
        public string ReviewText { get; set; }

        // Base interaction properties
        public Guid AggregateId { get; set; }
        public string ItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemType { get; set; }
        public string UserId { get; set; }

        // Navigation property
        public virtual InteractionAggregateEntity Interaction { get; set; }
    }

    public class RatingEntity
    {
        [Key]
        public Guid RatingId { get; set; }
        public float? Grade { get; set; }
        public float MinGrade { get; set; }
        public float MaxGrade { get; set; }
        public float? NormalizedGrade { get; set; }
        public bool IsComplexGrading { get; set; }

        // Base interaction properties
        public Guid AggregateId { get; set; }
        public string ItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemType { get; set; }
        public string UserId { get; set; }

        // Foreign key that points to either a GradeEntity or a GradingMethodInstanceEntity
        public Guid? GradableId { get; set; }
        public string GradableType { get; set; } // "grade" or "method"

        // Navigation properties
        public virtual InteractionAggregateEntity Interaction { get; set; }
    }

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
    }

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

        // Store components and actions as JSON
        [Column(TypeName = "jsonb")]
        public string ComponentsJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string ActionsJson { get; set; }

        // Helper methods for components and actions
        [NotMapped]
        public Dictionary<string, Guid> Components
        {
            get => string.IsNullOrEmpty(ComponentsJson)
                ? new Dictionary<string, Guid>()
                : JsonSerializer.Deserialize<Dictionary<string, Guid>>(ComponentsJson);
            set => ComponentsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> Actions
        {
            get => string.IsNullOrEmpty(ActionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ActionsJson);
            set => ActionsJson = JsonSerializer.Serialize(value);
        }
    }

    public class GradingBlockEntity
    {
        [Key]
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public float MinGrade { get; set; }
        public float MaxGrade { get; set; }
        public float? Grade { get; set; }
        public float? NormalizedGrade { get; set; }

        // Store components and actions as JSON
        [Column(TypeName = "jsonb")]
        public string ComponentsJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string ActionsJson { get; set; }

        // Helper methods for components and actions
        [NotMapped]
        public Dictionary<string, Guid> Components
        {
            get => string.IsNullOrEmpty(ComponentsJson)
                ? new Dictionary<string, Guid>()
                : JsonSerializer.Deserialize<Dictionary<string, Guid>>(ComponentsJson);
            set => ComponentsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> Actions
        {
            get => string.IsNullOrEmpty(ActionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ActionsJson);
            set => ActionsJson = JsonSerializer.Serialize(value);
        }
    }
}