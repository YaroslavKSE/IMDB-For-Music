using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetGradingMethodByIdUseCase : IRequestHandler<GetGradingMethodByIdCommand, GetGradingMethodDetailResult>
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public GetGradingMethodByIdUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        this.gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<GetGradingMethodDetailResult> Handle(GetGradingMethodByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the grading method
            var gradingMethod = await gradingMethodStorage.GetGradingMethodById(request.GradingMethodId);

            // Build the detailed DTO
            var detailDto = new GradingMethodDetailDto
            {
                Id = gradingMethod.SystemId,
                Name = gradingMethod.Name,
                CreatorId = gradingMethod.CreatorId,
                CreatedAt = gradingMethod.CreatedAt,
                IsPublic = gradingMethod.IsPublic,
                Components = new List<GradableComponentShowDto>(),
                Actions = ConvertActionsToStrings(gradingMethod.Actions),
                MinPossibleGrade = gradingMethod.getMin(),
                MaxPossibleGrade = gradingMethod.getMax()
            };

            // Convert all components
            for (int i = 0; i < gradingMethod.Grades.Count; i++)
            {
                detailDto.Components.Add(ConvertComponentToDto(gradingMethod.Grades[i]));
            }

            return new GetGradingMethodDetailResult
            {
                Success = true,
                GradingMethod = detailDto
            };
        }
        catch (Exception ex)
        {
            return new GetGradingMethodDetailResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                GradingMethod = null
            };
        }
    }

    // Recursive method to convert components to DTOs
    private GradableComponentShowDto ConvertComponentToDto(IGradable component)
    {
        if (component is Grade grade)
        {
            return new GradeDetailShowDto()
            {
                Name = grade.parametrName,
                MinPossibleGrade = grade.getMin(),
                MaxPossibleGrade = grade.getMax(),
                StepAmount = grade.stepAmount,
                Description = grade.Description
            };
        }
        else if (component is GradingBlock block)
        {
            var blockDto = new BlockDetailShowDto()
            {
                Name = block.BlockName,
                MinPossibleGrade = block.getMin(),
                MaxPossibleGrade = block.getMax(),
                Components = new List<GradableComponentShowDto>(),
                Actions = ConvertActionsToStrings(block.Actions)
            };

            for (int i = 0; i < block.Grades.Count; i++)
            {
                blockDto.Components.Add(ConvertComponentToDto(block.Grades[i]));
            }

            return blockDto;
        }

        throw new InvalidOperationException($"Unknown component type: {component.GetType().Name}");
    }

    // Helper to convert action enums to readable strings
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