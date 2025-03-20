using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

public class GetPublicGradingMethodsCommand : IRequest<GetGradingMethodsResult>
{ }

public class GetGradingMethodByIdCommand : IRequest<GetGradingMethodDetailResult>
{
    public Guid GradingMethodId { get; set; }
}

public class GetGradingMethodsResult
{
    public bool Success { get; set; }
    public List<GradingMethodSummaryDto> GradingMethods { get; set; }
    public string ErrorMessage { get; set; }
}

public class GetGradingMethodDetailResult
{
    public bool Success { get; set; }
    public GradingMethodDetailDto GradingMethod { get; set; }
    public string ErrorMessage { get; set; }
}

// Summary DTO for listing available methods
public class GradingMethodSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
}

// Detailed DTO for showing the full structure
public class GradingMethodDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public List<GradableComponentDto> Components { get; set; }
    public List<string> Actions { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}

// Base component DTO
[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeDetailDto), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(BlockDetailDto), typeDiscriminator: "block")]
public abstract class GradableComponentDto
{
    public string Name { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}

// Grade component details
public class GradeDetailDto : GradableComponentDto
{
    public float StepAmount { get; set; }
}

// Block component details with nested components
public class BlockDetailDto : GradableComponentDto
{
    public List<GradableComponentDto> Components { get; set; }
    public List<string> Actions { get; set; }
}

// Handler for getting all public grading methods
public class GetPublicGradingMethodsUseCase : IRequestHandler<GetPublicGradingMethodsCommand, GetGradingMethodsResult>
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public GetPublicGradingMethodsUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        this.gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<GetGradingMethodsResult> Handle(GetPublicGradingMethodsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = new GetGradingMethodsResult
            {
                GradingMethods = new List<GradingMethodSummaryDto>(),
                Success = true
            };

            var publicMethods = await gradingMethodStorage.GetPublicGradingMethods();

            // Convert to DTOs with summary information
            foreach (var method in publicMethods)
            {
                result.GradingMethods.Add(new GradingMethodSummaryDto
                {
                    Id = method.SystemId,
                    Name = method.Name,
                    CreatorId = method.CreatorId,
                    CreatedAt = method.CreatedAt,
                    IsPublic = method.IsPublic
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            return new GetGradingMethodsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                GradingMethods = new List<GradingMethodSummaryDto>()
            };
        }
    }
}

// Handler for getting detailed information about a specific grading method
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
                Components = new List<GradableComponentDto>(),
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
    private GradableComponentDto ConvertComponentToDto(IGradable component)
    {
        if (component is Grade grade)
        {
            return new GradeDetailDto()
            {
                Name = grade.parametrName,
                MinPossibleGrade = grade.getMin(),
                MaxPossibleGrade = grade.getMax(),
                StepAmount = grade.stepAmount
            };
        }
        else if (component is GradingBlock block)
        {
            var blockDto = new BlockDetailDto()
            {
                Name = block.BlockName,
                MinPossibleGrade = block.getMin(),
                MaxPossibleGrade = block.getMax(),
                Components = new List<GradableComponentDto>(),
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