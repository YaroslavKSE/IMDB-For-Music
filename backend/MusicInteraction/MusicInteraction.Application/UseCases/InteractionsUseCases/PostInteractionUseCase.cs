using System.Diagnostics;
using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

public class PostInteractionUseCase : IRequestHandler<PostInteractionCommand, PostInteractionResult>
{
    private readonly IInteractionStorage interactionStorage;
    private readonly IGradingMethodStorage gradingMethodStorage;

    public PostInteractionUseCase(IInteractionStorage interactionStorage, IGradingMethodStorage gradingMethodStorage)
    {
        this.interactionStorage = interactionStorage;
        this.gradingMethodStorage = gradingMethodStorage;
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
                bool gradingSuccessful = await ProcessComplexGrading(interaction, request.GradingMethodId.Value, request.GradeInputs);
                result.Graded = gradingSuccessful;

                if (!gradingSuccessful)
                {
                    result.ErrorMessage = "Failed to apply some or all grades";
                }
            }
            else if (request.BasicGrade.HasValue)
            {
                try
                {
                    // Process basic grading (original implementation)
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

    private async Task<bool> ProcessComplexGrading(InteractionsAggregate interaction, Guid gradingMethodId, List<GradeInputDTO> gradeInputs)
    {
        try
        {
            // Get the grading method template
            var gradingMethod = await gradingMethodStorage.GetGradingMethodById(gradingMethodId);

            // Apply the user's grades directly to the components
            bool allGradesApplied = ApplyGradesToGradingMethod(gradingMethod, gradeInputs);

            // Add the grading method to the interaction as a rating
            interaction.AddRating(gradingMethod);

            return allGradesApplied;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in complex grading: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private bool ApplyGradesToGradingMethod(GradingMethod gradingMethod, List<GradeInputDTO> inputs)
    {
        bool allGradesApplied = true;
        Dictionary<string, bool> appliedGrades = new Dictionary<string, bool>();

        // Track which inputs were used
        foreach (var input in inputs)
        {
            appliedGrades[input.ComponentName] = false;
        }

        // Apply grades to the root level components
        foreach (var gradable in gradingMethod.Grades)
        {
            bool applied = TryApplyGrade(gradable, inputs, "", appliedGrades);
            allGradesApplied = allGradesApplied && applied;
        }

        // Check if any inputs weren't applied
        foreach (var entry in appliedGrades)
        {
            if (!entry.Value)
            {
                Debug.WriteLine($"Warning: Grade for component '{entry.Key}' was not applied");
                allGradesApplied = false;
            }
        }

        return allGradesApplied;
    }

    private bool TryApplyGrade(IGradable gradable, List<GradeInputDTO> inputs, string parentPath, Dictionary<string, bool> appliedGrades)
    {
        bool allApplied = true;

        if (gradable is Grade grade)
        {
            string componentPath = string.IsNullOrEmpty(parentPath)
                ? grade.parametrName
                : $"{parentPath}.{grade.parametrName}";

            // Try to find a matching input
            var input = inputs.FirstOrDefault(i => string.Equals(i.ComponentName, componentPath, StringComparison.OrdinalIgnoreCase));
            if (input != null)
            {
                try
                {
                    // Apply the grade
                    grade.updateGrade(input.Value);
                    appliedGrades[input.ComponentName] = true;
                    Debug.WriteLine($"Successfully applied grade {input.Value} to component '{componentPath}'");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error applying grade to '{componentPath}': {ex.Message}");
                    allApplied = false;
                }
            }
            else
            {
                Debug.WriteLine($"No grade input found for component '{componentPath}'");
                allApplied = false;
            }
        }
        else if (gradable is GradingBlock block)
        {
            string blockPath = string.IsNullOrEmpty(parentPath)
                ? block.BlockName
                : $"{parentPath}.{block.BlockName}";

            // Process all grades in the block with the updated path
            foreach (var subGradable in block.Grades)
            {
                bool subApplied = TryApplyGrade(subGradable, inputs, blockPath, appliedGrades);
                allApplied = allApplied && subApplied;
            }
        }

        return allApplied;
    }
}