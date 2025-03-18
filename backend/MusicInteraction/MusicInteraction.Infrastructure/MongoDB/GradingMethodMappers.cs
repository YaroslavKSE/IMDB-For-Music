using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.MongoDB.Entities;

namespace MusicInteraction.Infrastructure.MongoDB.Mapping;

public static class GradingMethodMapper
{
    // Convert from Domain to Entity
    public static GradingMethodEntity ToEntity(this GradingMethod domainModel)
    {
        var entity = new GradingMethodEntity
        {
            Id = domainModel.SystemId,
            Name = domainModel.Name,
            CreatorId = domainModel.CreatorId,
            CreatedAt = domainModel.CreatedAt,
            IsPublic = domainModel.IsPublic,
            MinPossibleGrade = domainModel.getMin(),
            MaxPossibleGrade = domainModel.getMax(),
            // Store actions as integer values as strings to avoid deserialization issues
            Actions = domainModel.Actions.Select(a => ((int)a).ToString()).ToList(),
            Components = new List<GradableComponentEntity>()
        };

        // Map all components
        foreach (var component in domainModel.Grades)
        {
            entity.Components.Add(MapGradableToEntity(component));
        }

        return entity;
    }

    // Convert from Entity to Domain
    public static GradingMethod ToDomain(this GradingMethodEntity entity)
    {
        var domainModel = new GradingMethod(entity.Name, entity.CreatorId, entity.IsPublic);

        // Use reflection to set the private fields directly
        typeof(GradingMethod).GetProperty("SystemId")?.SetValue(domainModel, entity.Id);
        typeof(GradingMethod).GetProperty("CreatedAt")?.SetValue(domainModel, entity.CreatedAt);

        // Clear existing grades and actions
        domainModel.Grades.Clear();
        domainModel.Actions.Clear();

        // Add components and actions
        foreach (var component in entity.Components)
        {
            domainModel.AddGrade(MapEntityToGradable(component));
        }

        // Add actions between components
        for (int i = 0; i < entity.Actions.Count; i++)
        {
            domainModel.AddAction(ConvertStringToAction(entity.Actions[i]));
        }

        return domainModel;
    }

    // Map Gradable domain object to Entity
    private static GradableComponentEntity MapGradableToEntity(IGradable gradable)
    {
        if (gradable is Grade grade)
        {
            return new GradeEntity
            {
                Name = grade.parametrName,
                CurrentGrade = grade.getGrade(),
                MinPossibleGrade = grade.getMin(),
                MaxPossibleGrade = grade.getMax(),
                StepAmount = grade.stepAmount
            };
        }
        else if (gradable is GradingBlock block)
        {
            var blockEntity = new BlockEntity
            {
                Name = block.BlockName,
                CurrentGrade = block.getGrade(),
                MinPossibleGrade = block.getMin(),
                MaxPossibleGrade = block.getMax(),
                // Store actions as integer values as strings to avoid deserialization issues
                Actions = block.Actions.Select(a => ((int)a).ToString()).ToList(),
                Components = new List<GradableComponentEntity>()
            };

            // Add all sub-components
            foreach (var component in block.Grades)
            {
                blockEntity.Components.Add(MapGradableToEntity(component));
            }

            return blockEntity;
        }

        throw new InvalidOperationException($"Unknown gradable type: {gradable.GetType().Name}");
    }

    // Map Entity to Gradable domain object
    private static IGradable MapEntityToGradable(GradableComponentEntity entity)
    {
        if (entity is GradeEntity gradeEntity)
        {
            var grade = new Grade(
                gradeEntity.MinPossibleGrade,
                gradeEntity.MaxPossibleGrade,
                gradeEntity.StepAmount,
                gradeEntity.Name
            );

            // Set the grade if it exists
            if (gradeEntity.CurrentGrade.HasValue)
            {
                grade.updateGrade(gradeEntity.CurrentGrade.Value);
            }

            return grade;
        }
        else if (entity is BlockEntity blockEntity)
        {
            var block = new GradingBlock(blockEntity.Name);

            // Add all components
            foreach (var component in blockEntity.Components)
            {
                block.AddGrade(MapEntityToGradable(component));
            }

            // Add all actions
            foreach (var action in blockEntity.Actions)
            {
                block.AddAction(ConvertStringToAction(action));
            }

            return block;
        }

        throw new InvalidOperationException($"Unknown entity type: {entity.GetType().Name}");
    }

    // Helper to convert Action enum to string
    private static string ConvertActionToString(Domain.Action action)
    {
        switch (action)
        {
            case Domain.Action.Add:
                return "Add";
            case Domain.Action.Subtract:
                return "Subtract";
            case Domain.Action.Multiply:
                return "Multiply";
            case Domain.Action.Divide:
                return "Divide";
            default:
                throw new InvalidOperationException($"Unknown action enum value: {action}");
        }
    }

    // Helper to convert string to Action enum
    private static Domain.Action ConvertStringToAction(string actionStr)
    {
        switch (actionStr)
        {
            case "Add":
            case "0":
                return Domain.Action.Add;
            case "Subtract":
            case "1":
                return Domain.Action.Subtract;
            case "Multiply":
            case "2":
                return Domain.Action.Multiply;
            case "Divide":
            case "3":
                return Domain.Action.Divide;
            default:
                throw new InvalidOperationException($"Unknown action string: {actionStr}");
        }
    }
}