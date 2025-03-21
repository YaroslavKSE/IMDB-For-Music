using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetRatingByIdUseCase : IRequestHandler<GetRatingByIdCommand, GetRatingDetailResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetRatingByIdUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetRatingDetailResult> Handle(GetRatingByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await interactionStorage.IsEmpty())
            {
                return new GetRatingDetailResult
                {
                    Success = false,
                    ErrorMessage = "No ratings found"
                };
            }

            List<Rating> ratings = await interactionStorage.GetRatings();

            // Find the requested rating
            Rating rating = ratings.FirstOrDefault(r => r.RatingId == request.RatingId);

            if (rating == null)
            {
                return new GetRatingDetailResult
                {
                    Success = false,
                    ErrorMessage = $"Rating with ID {request.RatingId} not found"
                };
            }

            // Create detailed DTO
            var ratingDetail = new RatingDetailDTO
            {
                RatingId = rating.RatingId,
                ItemId = rating.ItemId,
                ItemType = rating.ItemType,
                UserId = rating.UserId,
                CreatedAt = rating.CreatedAt,
                NormalizedGrade = rating.Grade.getNormalizedGrade(),
                OverallGrade = rating.GetGrade(),
                MinPossibleGrade = rating.Grade.getMin(),
                MaxPossibleGrade = rating.Grade.getMax(),
                GradingComponent = ConvertComponentToDto(rating.Grade)
            };
            if (rating.Grade is GradingMethod method)
            {
                ratingDetail.GradingMethodId = method.SystemId;
            }

            return new GetRatingDetailResult
            {
                Success = true,
                Rating = ratingDetail
            };
        }
        catch (Exception ex)
        {
            return new GetRatingDetailResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving rating: {ex.Message}"
            };
        }
    }

    // Recursive method to convert components to DTOs
    private GradedComponentDTO ConvertComponentToDto(IGradable component)
    {
        if (component is Grade grade)
        {
            return new GradeDetailDTO
            {
                Name = grade.parametrName,
                CurrentGrade = grade.getGrade(),
                MinPossibleGrade = grade.getMin(),
                MaxPossibleGrade = grade.getMax(),
                StepAmount = grade.stepAmount,
                Description = grade.Description
            };
        }
        else if (component is GradingBlock block)
        {
            var blockDto = new GradedBlockDetailDTO
            {
                Name = block.BlockName,
                CurrentGrade = block.getGrade(),
                MinPossibleGrade = block.getMin(),
                MaxPossibleGrade = block.getMax(),
                Components = new List<GradedComponentDTO>(),
                Actions = ConvertActionsToStrings(block.Actions)
            };

            // Convert all sub-components
            foreach (var subComponent in block.Grades)
            {
                blockDto.Components.Add(ConvertComponentToDto(subComponent));
            }

            return blockDto;
        }

        throw new InvalidOperationException($"Unknown component type: {component.GetType().Name}");
    }

    private List<string> ConvertActionsToStrings(List<Domain.Action> actions)
    {
        var result = new List<string>();
        foreach (var action in actions)
        {
            switch (action)
            {
                case Domain.Action.Add:
                    result.Add("Add");
                    break;
                case Domain.Action.Subtract:
                    result.Add("Subtract");
                    break;
                case Domain.Action.Multiply:
                    result.Add("Multiply");
                    break;
                case Domain.Action.Divide:
                    result.Add("Divide");
                    break;
                default:
                    result.Add("Unknown");
                    break;
            }
        }
        return result;
    }
}