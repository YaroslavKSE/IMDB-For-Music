using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class UpdateGradingMethodUseCase : IRequestHandler<UpdateGradingMethodCommand, CreateGradingMethodResult>
{
    private readonly IGradingMethodStorage _gradingMethodStorage;

    public UpdateGradingMethodUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        _gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<CreateGradingMethodResult> Handle(UpdateGradingMethodCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _gradingMethodStorage.IsEmpty())
            {
                return new CreateGradingMethodResult
                {
                    Success = false,
                    ErrorMessage = "No grading methods found"
                };
            }

            var gradingMethodToUpdate = await _gradingMethodStorage.GetGradingMethodById(request.GradingMethodId);
            if (gradingMethodToUpdate == null)
            {
                return new CreateGradingMethodResult
                {
                    Success = false,
                    ErrorMessage = $"Grading method with ID {request.GradingMethodId} not found"
                };
            }

            var updatedGradingMethod = new GradingMethod(gradingMethodToUpdate.SystemId, gradingMethodToUpdate.CreatedAt, gradingMethodToUpdate.Name, gradingMethodToUpdate.CreatorId, request.IsPublic);
            for (int i = 0; i < request.Components.Count; i++)
            {
                var component = request.Components[i];
                IGradable gradableComponent = CreateGradableFromDto(component);

                updatedGradingMethod.AddGrade(gradableComponent);

                if (i < request.Components.Count - 1 && i < request.Actions.Count)
                {
                    updatedGradingMethod.AddAction(request.Actions[i]);
                }
            }

            await _gradingMethodStorage.UpdateGradingMethodAsync(updatedGradingMethod);

            return new CreateGradingMethodResult
            {
                Success = true,
                GradingMethodId = updatedGradingMethod.SystemId
            };

        }
        catch (Exception ex)
        {
            return new CreateGradingMethodResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private IGradable CreateGradableFromDto(ComponentDto component)
    {
        if (component is GradeComponentDto gradeDto)
        {
            return new Grade(
                gradeDto.MinGrade,
                gradeDto.MaxGrade,
                gradeDto.StepAmount,
                gradeDto.Name,
                gradeDto.Description
            );
        }
        else if (component is BlockComponentDto blockDto)
        {
            var block = new GradingBlock(blockDto.Name);

            // Process child components
            for (int i = 0; i < blockDto.SubComponents.Count; i++)
            {
                var childComponent = blockDto.SubComponents[i];
                IGradable gradableChild = CreateGradableFromDto(childComponent);

                block.AddGrade(gradableChild);

                if (i < blockDto.SubComponents.Count - 1 && i < blockDto.Actions.Count)
                {
                    block.AddAction(blockDto.Actions[i]);
                }
            }

            return block;
        }

        throw new ArgumentException($"Unsupported component type: {component.GetType().Name}");
    }
}