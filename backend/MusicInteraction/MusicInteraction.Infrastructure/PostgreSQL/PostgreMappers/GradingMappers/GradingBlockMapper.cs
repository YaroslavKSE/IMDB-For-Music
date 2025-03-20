using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class GradingBlockMapper
{
    public static async Task<Guid> MapToEntityAsync(GradingBlock block, MusicInteractionDbContext dbContext)
    {
        // Create and save the block entity
        var blockEntity = new GradingBlockEntity
        {
            EntityId = Guid.NewGuid(),
            Name = block.BlockName,
            MinGrade = block.getMin(),
            MaxGrade = block.getMax(),
            Grade = block.getGrade(),
            NormalizedGrade = block.getNormalizedGrade()
        };

        await dbContext.GradingBlocks.AddAsync(blockEntity);

        // Process components (recursively)
        for (int i = 0; i < block.Grades.Count; i++)
        {
            var component = block.Grades[i];
            GradingBlockComponentEntity componentLink;

            if (component is Grade grade)
            {
                // Store the grade
                var gradeEntity = await GradeMapper.MapToEntityAsync(grade, dbContext);

                // Create component link
                componentLink = new GradingBlockComponentEntity
                {
                    Id = Guid.NewGuid(),
                    GradingBlockId = blockEntity.EntityId,
                    ComponentType = "grade",
                    ComponentNumber = i,
                    GradeComponentId = gradeEntity.EntityId,
                    BlockComponentId = null
                };
            }
            else if (component is GradingBlock nestedBlock)
            {
                // Store the nested block recursively
                var nestedBlockId = await MapToEntityAsync(nestedBlock, dbContext);

                // Create component link
                componentLink = new GradingBlockComponentEntity
                {
                    Id = Guid.NewGuid(),
                    GradingBlockId = blockEntity.EntityId,
                    ComponentType = "block",
                    ComponentNumber = i,
                    GradeComponentId = null,
                    BlockComponentId = nestedBlockId
                };
            }
            else
            {
                throw new InvalidOperationException($"Unknown component type: {component.GetType().Name}");
            }

            await dbContext.GradingBlockComponents.AddAsync(componentLink);

            // Add action if it's not the last component
            if (i < block.Actions.Count)
            {
                var actionEntity = new GradingBlockActionEntity
                {
                    Id = Guid.NewGuid(),
                    GradingBlockId = blockEntity.EntityId,
                    ActionNumber = i,
                    ActionType = ActionMapper.ConvertActionToString(block.Actions[i])
                };

                await dbContext.GradingBlockActions.AddAsync(actionEntity);
            }
        }

        return blockEntity.EntityId;
    }

    public static async Task<GradingBlock> ToDomainAsync(Guid entityId, MusicInteractionDbContext dbContext)
    {
        var blockEntity = await dbContext.GradingBlocks
            .Include(b => b.Components)
            .Include(b => b.Actions)
            .FirstOrDefaultAsync(b => b.EntityId == entityId);

        if (blockEntity == null)
        {
            throw new Exception($"GradingBlock with ID {entityId} not found");
        }

        // Create a new instance of GradingBlock
        var block = new GradingBlock(blockEntity.Name);

        // Clear default components and actions
        block.Grades.Clear();
        block.Actions.Clear();

        // Process all components in order
        var componentsOrderedByNumber = blockEntity.Components.OrderBy(c => c.ComponentNumber).ToList();

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

                block.AddGrade(component);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading component: {ex.Message}");
                // Continue with other components to be resilient
                continue;
            }
        }

        // Add actions in order
        var actionsOrderedByNumber = blockEntity.Actions.OrderBy(a => a.ActionNumber).ToList();

        foreach (var action in actionsOrderedByNumber)
        {
            block.AddAction(ActionMapper.ConvertStringToAction(action.ActionType));
        }

        return block;
    }
}