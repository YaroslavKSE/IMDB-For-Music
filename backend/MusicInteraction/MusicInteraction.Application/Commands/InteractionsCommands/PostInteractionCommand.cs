using MediatR;

public class PostInteractionCommand : IRequest<PostInteractionResult>
{
    // Required fields
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }

    // Optional fields with default values
    public bool IsLiked { get; set; } = false;
    public string ReviewText { get; set; } = string.Empty;
    public bool UseComplexGrading { get; set; } = false;

    // Only used when UseComplexGrading = false
    public float? BasicGrade { get; set; } = null;

    // Only used when UseComplexGrading = true
    public Guid? GradingMethodId { get; set; } = null;
    public List<GradeInputDTO> GradeInputs { get; set; } = new List<GradeInputDTO>();
}