using System.Diagnostics;
using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class UpdateInteractionUseCase : IRequestHandler<UpdateInteractionCommand, UpdateInteractionResult>
{
    private readonly IInteractionStorage interactionStorage;
    private readonly IGradingMethodStorage gradingMethodStorage;
    private readonly ComplexInteractionGrader _interactionGrader;

    public UpdateInteractionUseCase(IInteractionStorage interactionStorage, IGradingMethodStorage gradingMethodStorage)
    {
        this.interactionStorage = interactionStorage;
        this.gradingMethodStorage = gradingMethodStorage;
        _interactionGrader = new ComplexInteractionGrader(gradingMethodStorage);
    }

    public async Task<UpdateInteractionResult> Handle(UpdateInteractionCommand request, CancellationToken cancellationToken)
    {
        var result = new UpdateInteractionResult()
        {
            InteractionUpdated = false,
            LikeUpdated = false,
            ReviewUpdated = false,
            GradingUpdated = false
        };

        var interactionToUpdate = interactionStorage.GetInteractionById(request.interactionId).Result;
        if (interactionToUpdate == null)
        {
            result.ErrorMessage = "interaction to update not found";
            return result;
        }

        result.InteractionId = interactionToUpdate.AggregateId;

        if (request.IsLiked != interactionToUpdate.IsLiked)
        {
            interactionToUpdate.IsLiked = request.IsLiked;
            result.LikeUpdated = true;
            result.InteractionUpdated = true;
        }

        if (interactionToUpdate.Review != null && request.ReviewText != interactionToUpdate.Review.ReviewText)
        {
            interactionToUpdate.Review.ReviewText = request.ReviewText;
            result.ReviewUpdated = true;
            result.InteractionUpdated = true;
        }
        else if (interactionToUpdate.Review == null && request.ReviewText != "")
        {
            interactionToUpdate.AddReview(request.ReviewText);
            result.ReviewUpdated = true;
            result.InteractionUpdated = true;
        }

        if (request.UpdateGrading)
        {
            if (request.UseComplexGrading && request.GradingMethodId.HasValue)
            {
                if (request.GradeInputs == null || request.GradeInputs.Count == 0)
                {
                    throw new ArgumentException("Complex grading requires grade inputs for components");
                }

                // Process complex grading
                bool gradingSuccessful = await _interactionGrader.ProcessComplexGrading(interactionToUpdate, request.GradingMethodId.Value, request.GradeInputs);
                result.GradingUpdated = gradingSuccessful;
                if (gradingSuccessful)
                {
                    result.GradingUpdated = gradingSuccessful;
                    result.InteractionUpdated = gradingSuccessful;
                }

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
                    interactionToUpdate.AddRating(grade);
                    result.GradingUpdated = true;
                    result.InteractionUpdated = true;
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Invalid basic grade: {ex.Message}";
                }
            }
        }

        await interactionStorage.UpdateInteractionAsync(interactionToUpdate);
        return result;
    }
}

