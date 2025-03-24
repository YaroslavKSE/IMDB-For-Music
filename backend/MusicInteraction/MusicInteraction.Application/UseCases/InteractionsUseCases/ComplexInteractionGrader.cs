using System.Diagnostics;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class ComplexInteractionGrader
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public ComplexInteractionGrader(IGradingMethodStorage _gradingMethodStorage)
    {
        gradingMethodStorage = _gradingMethodStorage;
    }

    public async Task<bool> ProcessComplexGrading(InteractionsAggregate interaction, Guid gradingMethodId, List<GradeInputDTO> gradeInputs)
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