using MediatR;

namespace MusicInteraction.Application;

public class UpdateInteractionCommand : IRequest<UpdateInteractionResult>
{
    // Required fields
    public Guid interactionId { get; set; }

    // Optional fields with default values
    public bool IsLiked { get; set; } = false;
    public string ReviewText { get; set; } = string.Empty;
    public bool UpdateGrading { get; set; } = false;
    public bool UseComplexGrading { get; set; } = false;

    // Only used when UseComplexGrading = false
    public float? BasicGrade { get; set; } = null;

    // Only used when UseComplexGrading = true
    public Guid? GradingMethodId { get; set; } = null;
    public List<GradeInputDTO> GradeInputs { get; set; } = new List<GradeInputDTO>();
}