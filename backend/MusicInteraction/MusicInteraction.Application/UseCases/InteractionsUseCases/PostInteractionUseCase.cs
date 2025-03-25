using System.Diagnostics;
using MediatR;
using MusicInteraction.Application;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

public class PostInteractionUseCase : IRequestHandler<PostInteractionCommand, PostInteractionResult>
{
    private readonly IInteractionStorage interactionStorage;
    private readonly IGradingMethodStorage gradingMethodStorage;
    private readonly ComplexInteractionGrader interactionGrader;

    public PostInteractionUseCase(IInteractionStorage interactionStorage, IGradingMethodStorage gradingMethodStorage)
    {
        this.interactionStorage = interactionStorage;
        this.gradingMethodStorage = gradingMethodStorage;
        interactionGrader = new ComplexInteractionGrader(gradingMethodStorage);
    }

    public async Task<PostInteractionResult> Handle(PostInteractionCommand request, CancellationToken cancellationToken)
    {
        var result = new PostInteractionResult()
        {
            InteractionCreated = false,
            Liked = false,
            ReviewCreated = false,
            Graded = false
        };

        var interaction = new InteractionsAggregate(request.UserId, request.ItemId, request.ItemType);
        result.InteractionCreated = true;
        result.InteractionId = interaction.AggregateId;

        try
        {
            // Handle grading based on the selected method
            if (request.UseComplexGrading && request.GradingMethodId.HasValue)
            {
                if (request.GradeInputs == null || request.GradeInputs.Count == 0)
                {
                    throw new ArgumentException("Complex grading requires grade inputs for components");
                }

                // Process complex grading
                bool gradingSuccessful = await interactionGrader.ProcessComplexGrading(interaction, request.GradingMethodId.Value, request.GradeInputs);
                result.Graded = gradingSuccessful;

                if (!gradingSuccessful)
                {
                    result.ErrorMessage = "Failed to apply some or all grades";
                }
            }
            else if (!request.UseComplexGrading && request.BasicGrade.HasValue)
            {
                try
                {
                    // Process basic grading
                    var grade = new Grade();
                    grade.updateGrade(request.BasicGrade.Value);
                    interaction.AddRating(grade);
                    result.Graded = true;
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Invalid basic grade: {ex.Message}";
                }
            }
            // If UseComplexGrading is false and BasicGrade is null, no rating is created

            if (request.IsLiked)
            {
                interaction.AddLike();
                result.Liked = true;
            }

            if (!string.IsNullOrEmpty(request.ReviewText))
            {
                interaction.AddReview(request.ReviewText);
                result.ReviewCreated = true;
            }

            await interactionStorage.AddInteractionAsync(interaction);
        }
        catch (Exception ex)
        {
            // Log the error
            Debug.WriteLine($"Error in PostInteractionUseCase: {ex.Message}");
            result.ErrorMessage = $"Error processing interaction: {ex.Message}";
        }

        return result;
    }
}