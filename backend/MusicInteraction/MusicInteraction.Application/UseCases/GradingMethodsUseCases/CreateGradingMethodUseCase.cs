using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class CreateGradingMethodUseCase : IRequestHandler<CreateGradingMethodCommand, CreateGradingMethodResult>
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public CreateGradingMethodUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        this.gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<CreateGradingMethodResult> Handle(CreateGradingMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var gradingMethod = new GradingMethod(request.Name, request.UserId, request.IsPublic);

            // Process the components and add them to the grading method
            for (int i = 0; i < request.Components.Count; i++)
            {
                var component = request.Components[i];
                IGradable gradableComponent = CreateGradableFromDto(component);

                gradingMethod.AddGrade(gradableComponent);

                if (i < request.Components.Count - 1 && i < request.Actions.Count)
                {
                    gradingMethod.AddAction(request.Actions[i]);
                }
            }

            await gradingMethodStorage.AddGradingMethodAsync(gradingMethod);

            return new CreateGradingMethodResult
            {
                Success = true,
                GradingMethodId = gradingMethod.SystemId
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