using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Navigation properties
        public virtual InteractionAggregateEntity Interaction { get; set; }

        // One-to-one relationship with GradeEntity
        public virtual GradeEntity SimpleGrade { get; set; }

        // One-to-one relationship with GradingMethodInstanceEntity
        public virtual GradingMethodInstanceEntity ComplexGrade { get; set; }
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

        // This field can be null when the grade is part of a grading block
        public Guid? RatingId { get; set; }

        // Navigation property (optional)
        [ForeignKey("RatingId")]
        public virtual RatingEntity Rating { get; set; }

        // Navigation for reverse relationships
        public virtual ICollection<GradingMethodComponentEntity> MethodComponents { get; set; }
        public virtual ICollection<GradingBlockComponentEntity> BlockComponents { get; set; }
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

        // This can be null for standalone grading method instances
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

    // New entity for linking grading methods to their components
    public class GradingMethodComponentEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to the parent grading method
        public Guid GradingMethodId { get; set; }

        // Component type (block or grade)
        public string ComponentType { get; set; }

        // Order of the component in the method
        public int ComponentNumber { get; set; }

        // Foreign keys to the actual components (one will be null)
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

    // New entity for linking grading blocks to their components
    public class GradingBlockComponentEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to the parent grading block
        public Guid GradingBlockId { get; set; }

        // Component type (block or grade)
        public string ComponentType { get; set; }

        // Order of the component in the block
        public int ComponentNumber { get; set; }

        // Foreign keys to the actual components (one will be null)
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

    // New entity for storing grading method actions
    public class GradingMethodActionEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to the parent grading method
        public Guid GradingMethodId { get; set; }

        // Order of the action in the method
        public int ActionNumber { get; set; }

        // The action type (+, -, *, /)
        public string ActionType { get; set; }

        // Navigation property
        [ForeignKey("GradingMethodId")]
        public virtual GradingMethodInstanceEntity GradingMethod { get; set; }
    }

    // New entity for storing grading block actions
    public class GradingBlockActionEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to the parent grading block
        public Guid GradingBlockId { get; set; }

        // Order of the action in the block
        public int ActionNumber { get; set; }

        // The action type (+, -, *, /)
        public string ActionType { get; set; }

        // Navigation property
        [ForeignKey("GradingBlockId")]
        public virtual GradingBlockEntity GradingBlock { get; set; }
    }
}