using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class GradingMethodMapper
{
    public static async Task<Guid> MapToEntityAsync(GradingMethod method, MusicInteractionDbContext dbContext, Guid? ratingId = null)
    {
        // Create and save the method entity
        var methodEntity = new GradingMethodInstanceEntity
        {
            EntityId = Guid.NewGuid(),
            MethodId = method.SystemId,
            Name = method.Name,
            MinGrade = method.getMin(),
            MaxGrade = method.getMax(),
            Grade = method.getGrade(),
            NormalizedGrade = method.getNormalizedGrade(),
            RatingId = ratingId
        };

        await dbContext.GradingMethodInstances.AddAsync(methodEntity);

        // Process components
        for (int i = 0; i < method.Grades.Count; i++)
        {
            var component = method.Grades[i];
            GradingMethodComponentEntity componentLink;

            if (component is Grade grade)
            {
                // Store the grade
                var gradeEntity = await GradeMapper.MapToEntityAsync(grade, dbContext);

                // Create component link
                componentLink = new GradingMethodComponentEntity
                {
                    Id = Guid.NewGuid(),
                    GradingMethodId = methodEntity.EntityId,
                    ComponentType = "grade",
                    ComponentNumber = i,
                    GradeComponentId = gradeEntity.EntityId,
                    BlockComponentId = null
                };
            }
            else if (component is GradingBlock block)
            {
                // Store the block
                var blockEntityId = await GradingBlockMapper.MapToEntityAsync(block, dbContext);

                // Create component link
                componentLink = new GradingMethodComponentEntity
                {
                    Id = Guid.NewGuid(),
                    GradingMethodId = methodEntity.EntityId,
                    ComponentType = "block",
                    ComponentNumber = i,
                    GradeComponentId = null,
                    BlockComponentId = blockEntityId
                };
            }
            else
            {
                throw new InvalidOperationException($"Unknown component type: {component.GetType().Name}");
            }

            await dbContext.GradingMethodComponents.AddAsync(componentLink);

            // Add action if it's not the last component
            if (i < method.Actions.Count)
            {
                var actionEntity = new GradingMethodActionEntity
                {
                    Id = Guid.NewGuid(),
                    GradingMethodId = methodEntity.EntityId,
                    ActionNumber = i,
                    ActionType = ActionMapper.ConvertActionToString(method.Actions[i])
                };

                await dbContext.GradingMethodActions.AddAsync(actionEntity);
            }
        }

        return methodEntity.EntityId;
    }

    public static async Task<GradingMethod> GetByRatingIdAsync(Guid ratingId, MusicInteractionDbContext dbContext)
    {
        var methodEntity = await dbContext.GradingMethodInstances
            .FirstOrDefaultAsync(m => m.RatingId == ratingId);

        if (methodEntity == null)
        {
            // Fallback to a simple grade if no method exists
            return new GradingMethod("Default Method", "system", false);
        }

        return await LoadGradingMethodFromEntityIdAsync(methodEntity.EntityId, dbContext);
    }

    public static async Task<GradingMethod> LoadGradingMethodFromEntityIdAsync(Guid entityId, MusicInteractionDbContext dbContext)
    {
        var methodEntity = await dbContext.GradingMethodInstances
            .Include(m => m.Components)
            .Include(m => m.Actions)
            .FirstOrDefaultAsync(m => m.EntityId == entityId);

        if (methodEntity == null)
        {
            throw new Exception($"GradingMethodInstance with ID {entityId} not found");
        }

        // Create a new instance of GradingMethod
        var method = new GradingMethod(methodEntity.Name, "system", false);

        // Use reflection to set private fields
        typeof(GradingMethod).GetProperty("SystemId")?.SetValue(method, methodEntity.MethodId);

        // Clear default components and actions
        method.Grades.Clear();
        method.Actions.Clear();

        // Process all components in order
        var componentsOrderedByNumber = methodEntity.Components.OrderBy(c => c.ComponentNumber).ToList();

        foreach (var componentLink in componentsOrderedByNumber)
        {
            try
            {
                IGradable component;

                if (componentLink.ComponentType == "grade" && componentLink.GradeComponentId.HasValue)
                {
                    component = await GradeMapper.GetByEntityIdAsync(componentLink.GradeComponentId.Value, dbContext);
                }
                else if (componentLink.ComponentType == "block" && componentLink.BlockComponentId.HasValue)
                {
                    component = await GradingBlockMapper.ToDomainAsync(componentLink.BlockComponentId.Value, dbContext);
                }
                else
                {
                    Console.WriteLine($"Invalid component link: Type={componentLink.ComponentType}, GradeId={componentLink.GradeComponentId}, BlockId={componentLink.BlockComponentId}");
                    continue;
                }

                method.AddGrade(component);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading component: {ex.Message}");
                continue;
            }
        }

        // Add actions in order
        var actionsOrderedByNumber = methodEntity.Actions.OrderBy(a => a.ActionNumber).ToList();

        foreach (var action in actionsOrderedByNumber)
        {
            method.AddAction(ActionMapper.ConvertStringToAction(action.ActionType));
        }

        return method;
    }
}