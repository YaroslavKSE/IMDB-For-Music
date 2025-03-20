using MediatR;

public class PostInteractionCommand : IRequest<PostInteractionResult>
{
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public bool IsLiked { get; set; }
    public string ReviewText { get; set; }
    public bool UseComplexGrading { get; set; } = false;

    // For basic grading
    public float? BasicGrade { get; set; }

    // For complex grading
    public Guid? GradingMethodId { get; set; }
    public List<GradeInputDTO> GradeInputs { get; set; }
}