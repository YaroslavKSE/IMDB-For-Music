using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public static class GradingMethodBuilder
{
    public static void BuildGradingMethod(List<ComponentDto> components, List<Domain.Action> actions, GradingMethod gradingMethod)
    {
        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            IGradable gradableComponent = CreateGradableFromDto(component);

            gradingMethod.AddGrade(gradableComponent);

            if (i < components.Count - 1 && i < actions.Count)
            {
                gradingMethod.AddAction(actions[i]);
            }
        }
        //return gradingMethod;
    }

    private static IGradable CreateGradableFromDto(ComponentDto component)
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