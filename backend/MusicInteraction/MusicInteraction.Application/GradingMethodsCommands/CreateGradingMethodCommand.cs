using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

public class CreateGradingMethodCommand : IRequest<CreateGradingMethodResult>
{
    public string Name { get; set; }
    public string UserId { get; set; }
    public bool IsPublic { get; set; }
    public List<ComponentDto> Components { get; set; }
    public List<Domain.Action> Actions { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeComponentDto), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(BlockComponentDto), typeDiscriminator: "block")]
public abstract class ComponentDto
{
    public string Name { get; set; }
}

public class GradeComponentDto : ComponentDto
{
    public float MinGrade { get; set; }
    public float MaxGrade { get; set; }
    public float StepAmount { get; set; }
}

public class BlockComponentDto : ComponentDto
{
    public List<ComponentDto> SubComponents { get; set; }
    public List<Domain.Action> Actions { get; set; }
}

public class CreateGradingMethodResult
{
    public bool Success { get; set; }
    public Guid? GradingMethodId { get; set; }
    public string ErrorMessage { get; set; }
}

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
            // Create the grading method
            var gradingMethod = new GradingMethod(request.Name, request.UserId, request.IsPublic);

            // Process the components and add them to the grading method
            for (int i = 0; i < request.Components.Count; i++)
            {
                var component = request.Components[i];
                IGradable gradableComponent = CreateGradableFromDto(component);

                // Add the component to the grading method
                gradingMethod.AddGrade(gradableComponent);

                // Add actions between components (except after the last component)
                if (i < request.Components.Count - 1 && i < request.Actions.Count)
                {
                    gradingMethod.AddAction(request.Actions[i]);
                }
            }

            // Save the grading method
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
                gradeDto.Name
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

                // Add to block
                block.AddGrade(gradableChild);

                // Add actions between components (except after the last one)
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