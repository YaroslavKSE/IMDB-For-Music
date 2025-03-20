using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping
{
    public static class InteractionMapper
    {
        public static InteractionAggregateEntity ToEntity(InteractionsAggregate domain)
        {
            var entity = new InteractionAggregateEntity
            {
                AggregateId = domain.AggregateId,
                UserId = domain.UserId,
                ItemId = domain.ItemId,
                ItemType = domain.ItemType,
                CreatedAt = domain.CreatedAt,
                IsLiked = domain.IsLiked,
            };

            return entity;
        }

        public static async Task<InteractionsAggregate> ToDomain(InteractionAggregateEntity entity,
            MusicInteractionDbContext dbContext)
        {
            // Create the interaction aggregate with the basic properties
            var domain = new InteractionsAggregate(entity.UserId, entity.ItemId, entity.ItemType);

            // Use reflection to set the private fields that can't be set through constructor
            typeof(InteractionsAggregate).GetProperty("AggregateId")?.SetValue(domain, entity.AggregateId);
            typeof(InteractionsAggregate).GetProperty("CreatedAt")?.SetValue(domain, entity.CreatedAt);
            typeof(InteractionsAggregate).GetProperty("IsLiked")?.SetValue(domain, entity.IsLiked);

            // Load review if exists - use the navigation property or query by AggregateId
            var reviewEntity = await dbContext.Reviews.FirstOrDefaultAsync(r => r.AggregateId == entity.AggregateId);
            if (reviewEntity != null)
            {
                typeof(InteractionsAggregate).GetProperty("Review")?.SetValue(
                    domain,
                    ReviewMapper.ToDomain(reviewEntity, dbContext).Result
                );
            }

            try {
                // Load rating if exists - use the navigation property or query by AggregateId
                var ratingEntity = await dbContext.Ratings.FirstOrDefaultAsync(r => r.AggregateId == entity.AggregateId);
                if (ratingEntity != null)
                {
                    typeof(InteractionsAggregate).GetProperty("Rating")?.SetValue(
                        domain,
                        await RatingMapper.ToDomain(ratingEntity, dbContext)
                    );
                }
            }
            catch (Exception ex) {
                // Log the error but don't fail the whole request
                Console.WriteLine($"Error loading rating for interaction {entity.AggregateId}: {ex.Message}");
                // We could set a "placeholder" rating here if needed
            }

            return domain;
        }
    }

    public static class ReviewMapper
    {
        public static ReviewEntity ToEntity(Review domain)
        {
            return new ReviewEntity
            {
                ReviewId = domain.ReviewId,
                ReviewText = domain.ReviewText,
                AggregateId = domain.AggregateId,
            };
        }

        public static async Task<Review> ToDomain(ReviewEntity entity, MusicInteractionDbContext dbContext)
        {
            if (entity.Interaction == null)
            {
                entity.Interaction = await dbContext.Interactions
                    .FirstOrDefaultAsync(i => i.AggregateId == entity.AggregateId);
            }

            var review = new Review(
                entity.ReviewText,
                entity.AggregateId,
                entity.Interaction.ItemId,
                entity.Interaction.CreatedAt,
                entity.Interaction.ItemType,
                entity.Interaction.UserId
            );
            review.ReviewId = entity.ReviewId;
            return review;
        }
    }

    public static class RatingMapper
    {
        public class RatingMappingResult
        {
            public RatingEntity RatingEntity { get; set; }
        }

        public static async Task<RatingMappingResult> ToEntityWithGradables(Rating domain,
            MusicInteractionDbContext dbContext)
        {
            var ratingEntity = new RatingEntity
            {
                RatingId = domain.RatingId,
                AggregateId = domain.AggregateId,
                IsComplexGrading = domain.Grade is not Grade
            };

            await dbContext.Ratings.AddAsync(ratingEntity);

            // Handle different types of gradable components
            if (domain.Grade is Grade grade)
            {
                ratingEntity.IsComplexGrading = false;

                // Create and save the grade entity
                var gradeEntity = new GradeEntity
                {
                    EntityId = Guid.NewGuid(),
                    Name = grade.parametrName,
                    MinGrade = grade.getMin(),
                    MaxGrade = grade.getMax(),
                    Grade = grade.getGrade(),
                    StepAmount = grade.stepAmount,
                    NormalizedGrade = grade.getNormalizedGrade(),
                    RatingId = ratingEntity.RatingId
                };

                await dbContext.Grades.AddAsync(gradeEntity);
            }
            else if (domain.Grade is GradingMethod gradingMethod)
            {
                ratingEntity.IsComplexGrading = true;

                // Process this complex grading method
                var methodEntityId = await StoreGradingMethodInstance(gradingMethod, dbContext, ratingEntity.RatingId);
            }

            return new RatingMappingResult {RatingEntity = ratingEntity};
        }

        public static async Task<Rating> ToDomain(RatingEntity entity, MusicInteractionDbContext dbContext)
        {
            // First, reconstruct the gradable component
            IGradable gradable;

            if (!entity.IsComplexGrading)
            {
                var gradeEntity = await dbContext.Grades
                    .FirstOrDefaultAsync(g => g.RatingId == entity.RatingId);
                if (gradeEntity != null)
                {
                    gradable = new Grade(
                        gradeEntity.MinGrade,
                        gradeEntity.MaxGrade,
                        gradeEntity.StepAmount,
                        gradeEntity.Name
                    );

                    if (gradeEntity.Grade.HasValue)
                    {
                        ((Grade)gradable).updateGrade(gradeEntity.Grade.Value);
                    }
                }
                else
                {
                    gradable = new Grade();
                }
            }
            else
            {
                var methodEntity = await dbContext.GradingMethodInstances
                    .FirstOrDefaultAsync(m => m.RatingId == entity.RatingId);
                if (methodEntity != null)
                {
                    gradable = await LoadGradingMethodInstance(methodEntity.EntityId, dbContext);
                }
                else {
                    gradable = new Grade();
                }
            }

            if (entity.Interaction == null)
            {
                entity.Interaction = await dbContext.Interactions
                    .FirstOrDefaultAsync(i => i.AggregateId == entity.AggregateId);
            }

            // Create the rating with the reconstructed gradable
            var rating = new Rating(
                gradable,
                entity.AggregateId,
                entity.Interaction.ItemId,
                entity.Interaction.CreatedAt,
                entity.Interaction.ItemType,
                entity.Interaction.UserId
            );

            rating.RatingId = entity.RatingId;
            return rating;
        }

        // Helper method to store a grading method instance
        private static async Task<Guid> StoreGradingMethodInstance(GradingMethod method,
            MusicInteractionDbContext dbContext, Guid? ratingId = null)
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

            // Process components (recursively)
            for (int i = 0; i < method.Grades.Count; i++)
            {
                var component = method.Grades[i];

                if (component is Grade grade)
                {
                    // Store the grade
                    var gradeEntity = new GradeEntity
                    {
                        EntityId = Guid.NewGuid(),
                        Name = grade.parametrName,
                        MinGrade = grade.getMin(),
                        MaxGrade = grade.getMax(),
                        Grade = grade.getGrade(),
                        StepAmount = grade.stepAmount,
                        NormalizedGrade = grade.getNormalizedGrade()
                    };

                    await dbContext.Grades.AddAsync(gradeEntity);

                    // Create component link
                    var componentLink = new GradingMethodComponentEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingMethodId = methodEntity.EntityId,
                        ComponentType = "grade",
                        ComponentNumber = i,
                        GradeComponentId = gradeEntity.EntityId,
                        BlockComponentId = null
                    };

                    await dbContext.GradingMethodComponents.AddAsync(componentLink);
                }
                else if (component is GradingBlock block)
                {
                    // Store the block
                    var blockEntityId = await StoreGradingBlock(block, dbContext);

                    // Create component link
                    var componentLink = new GradingMethodComponentEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingMethodId = methodEntity.EntityId,
                        ComponentType = "block",
                        ComponentNumber = i,
                        GradeComponentId = null,
                        BlockComponentId = blockEntityId
                    };

                    await dbContext.GradingMethodComponents.AddAsync(componentLink);
                }

                // Add action if it's not the last component
                if (i < method.Actions.Count)
                {
                    var actionEntity = new GradingMethodActionEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingMethodId = methodEntity.EntityId,
                        ActionNumber = i,
                        ActionType = ConvertActionToString(method.Actions[i])
                    };

                    await dbContext.GradingMethodActions.AddAsync(actionEntity);
                }
            }

            return methodEntity.EntityId;
        }

        // Helper method to load a grading method instance
        private static async Task<GradingMethod> LoadGradingMethodInstance(Guid entityId,
            MusicInteractionDbContext dbContext)
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
                IGradable component;

                if (componentLink.ComponentType == "grade" && componentLink.GradeComponentId.HasValue)
                {
                    try {
                        // Load the grade
                        var gradeEntity = await dbContext.Grades.FindAsync(componentLink.GradeComponentId.Value);
                        if (gradeEntity == null)
                        {
                            Console.WriteLine($"Warning: Grade with ID {componentLink.GradeComponentId.Value} not found");
                            continue;
                        }

                        var grade = new Grade(
                            gradeEntity.MinGrade,
                            gradeEntity.MaxGrade,
                            gradeEntity.StepAmount,
                            gradeEntity.Name
                        );

                        if (gradeEntity.Grade.HasValue)
                        {
                            grade.updateGrade(gradeEntity.Grade.Value);
                        }

                        component = grade;
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error loading grade {componentLink.GradeComponentId}: {ex.Message}");
                        continue;
                    }
                }
                else if (componentLink.ComponentType == "block" && componentLink.BlockComponentId.HasValue)
                {
                    try {
                        component = await LoadGradingBlock(componentLink.BlockComponentId.Value, dbContext);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error loading block {componentLink.BlockComponentId}: {ex.Message}");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid component link: Type={componentLink.ComponentType}, GradeId={componentLink.GradeComponentId}, BlockId={componentLink.BlockComponentId}");
                    continue;
                }

                method.AddGrade(component);
            }

            // Add actions in order
            var actionsOrderedByNumber = methodEntity.Actions.OrderBy(a => a.ActionNumber).ToList();

            foreach (var action in actionsOrderedByNumber)
            {
                method.AddAction(ConvertStringToAction(action.ActionType));
            }

            return method;
        }

        // Helper method to store a grading block
        private static async Task<Guid> StoreGradingBlock(GradingBlock block, MusicInteractionDbContext dbContext)
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

                if (component is Grade grade)
                {
                    // Store the grade
                    var gradeEntity = new GradeEntity
                    {
                        EntityId = Guid.NewGuid(),
                        Name = grade.parametrName,
                        MinGrade = grade.getMin(),
                        MaxGrade = grade.getMax(),
                        Grade = grade.getGrade(),
                        StepAmount = grade.stepAmount,
                        NormalizedGrade = grade.getNormalizedGrade()
                        // No RatingId as this is part of a block
                    };

                    await dbContext.Grades.AddAsync(gradeEntity);

                    // Create component link
                    var componentLink = new GradingBlockComponentEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingBlockId = blockEntity.EntityId,
                        ComponentType = "grade",
                        ComponentNumber = i,
                        GradeComponentId = gradeEntity.EntityId,
                        BlockComponentId = null
                    };

                    await dbContext.GradingBlockComponents.AddAsync(componentLink);
                }
                else if (component is GradingBlock nestedBlock)
                {
                    // Store the nested block
                    var nestedBlockId = await StoreGradingBlock(nestedBlock, dbContext);

                    // Create component link
                    var componentLink = new GradingBlockComponentEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingBlockId = blockEntity.EntityId,
                        ComponentType = "block",
                        ComponentNumber = i,
                        GradeComponentId = null,
                        BlockComponentId = nestedBlockId
                    };

                    await dbContext.GradingBlockComponents.AddAsync(componentLink);
                }

                // Add action if it's not the last component
                if (i < block.Actions.Count)
                {
                    var actionEntity = new GradingBlockActionEntity
                    {
                        Id = Guid.NewGuid(),
                        GradingBlockId = blockEntity.EntityId,
                        ActionNumber = i,
                        ActionType = ConvertActionToString(block.Actions[i])
                    };

                    await dbContext.GradingBlockActions.AddAsync(actionEntity);
                }
            }

            return blockEntity.EntityId;
        }

        // Helper method to load a grading block
        private static async Task<GradingBlock> LoadGradingBlock(Guid entityId, MusicInteractionDbContext dbContext)
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
                IGradable component;

                if (componentLink.ComponentType == "grade" && componentLink.GradeComponentId.HasValue)
                {
                    try {
                        // Load the grade
                        var gradeEntity = await dbContext.Grades.FindAsync(componentLink.GradeComponentId.Value);
                        if (gradeEntity == null)
                        {
                            Console.WriteLine($"Warning: Grade with ID {componentLink.GradeComponentId.Value} not found");
                            continue;
                        }

                        var grade = new Grade(
                            gradeEntity.MinGrade,
                            gradeEntity.MaxGrade,
                            gradeEntity.StepAmount,
                            gradeEntity.Name
                        );

                        if (gradeEntity.Grade.HasValue)
                        {
                            grade.updateGrade(gradeEntity.Grade.Value);
                        }

                        component = grade;
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error loading grade {componentLink.GradeComponentId}: {ex.Message}");
                        continue;
                    }
                }
                else if (componentLink.ComponentType == "block" && componentLink.BlockComponentId.HasValue)
                {
                    try {
                        component = await LoadGradingBlock(componentLink.BlockComponentId.Value, dbContext);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error loading nested block {componentLink.BlockComponentId}: {ex.Message}");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid component link: Type={componentLink.ComponentType}, GradeId={componentLink.GradeComponentId}, BlockId={componentLink.BlockComponentId}");
                    continue;
                }

                block.AddGrade(component);
            }

            // Add actions in order
            var actionsOrderedByNumber = blockEntity.Actions.OrderBy(a => a.ActionNumber).ToList();

            foreach (var action in actionsOrderedByNumber)
            {
                block.AddAction(ConvertStringToAction(action.ActionType));
            }

            return block;
        }

        // Helper to convert Action enum to string
        private static string ConvertActionToString(Domain.Action action)
        {
            switch (action)
            {
                case Domain.Action.Add:
                    return "+";
                case Domain.Action.Subtract:
                    return "-";
                case Domain.Action.Multiply:
                    return "*";
                case Domain.Action.Divide:
                    return "/";
                default:
                    throw new InvalidOperationException($"Unknown action enum value: {action}");
            }
        }

        // Helper to convert string to Action enum
        private static Domain.Action ConvertStringToAction(string actionStr)
        {
            switch (actionStr)
            {
                case "+":
                case "Add":
                case "0":
                    return Domain.Action.Add;
                case "-":
                case "Subtract":
                case "1":
                    return Domain.Action.Subtract;
                case "*":
                case "Multiply":
                case "2":
                    return Domain.Action.Multiply;
                case "/":
                case "Divide":
                case "3":
                    return Domain.Action.Divide;
                default:
                    throw new InvalidOperationException($"Unknown action string: {actionStr}");
            }
        }
    }
}