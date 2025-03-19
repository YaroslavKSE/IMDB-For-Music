using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

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
                    ReviewMapper.ToDomain(reviewEntity)
                );
            }

            // Load rating if exists - use the navigation property or query by AggregateId
            var ratingEntity = await dbContext.Ratings.FirstOrDefaultAsync(r => r.AggregateId == entity.AggregateId);
            if (ratingEntity != null)
            {
                typeof(InteractionsAggregate).GetProperty("Rating")?.SetValue(
                    domain,
                    await RatingMapper.ToDomain(ratingEntity, dbContext)
                );
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
                ItemId = domain.ItemId,
                CreatedAt = domain.CreatedAt,
                ItemType = domain.ItemType,
                UserId = domain.UserId
            };
        }

        public static Review ToDomain(ReviewEntity entity)
        {
            var review = new Review(
                entity.ReviewText,
                entity.AggregateId,
                entity.ItemId,
                entity.CreatedAt,
                entity.ItemType,
                entity.UserId
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
                Grade = domain.GetGrade(),
                MinGrade = domain.Grade.getMin(),
                MaxGrade = domain.Grade.getMax(),
                NormalizedGrade = domain.Grade.getNormalizedGrade(),
                AggregateId = domain.AggregateId,
                ItemId = domain.ItemId,
                CreatedAt = domain.CreatedAt,
                ItemType = domain.ItemType,
                UserId = domain.UserId
            };

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
                    NormalizedGrade = grade.getNormalizedGrade()
                };

                await dbContext.Grades.AddAsync(gradeEntity);
                await dbContext.SaveChangesAsync();

                ratingEntity.GradableId = gradeEntity.EntityId;
                ratingEntity.GradableType = "grade";
            }
            else if (domain.Grade is GradingMethod gradingMethod)
            {
                ratingEntity.IsComplexGrading = true;

                // Process this complex grading method
                var methodEntityId = await StoreGradingMethodInstance(gradingMethod, dbContext);

                ratingEntity.GradableId = methodEntityId;
                ratingEntity.GradableType = "method";
            }
            else if (domain.Grade is GradingBlock gradingBlock)
            {
                ratingEntity.IsComplexGrading = true;

                // Process this grading block
                var blockEntityId = await StoreGradingBlock(gradingBlock, dbContext);

                ratingEntity.GradableId = blockEntityId;
                ratingEntity.GradableType = "block";
            }

            return new RatingMappingResult {RatingEntity = ratingEntity};
        }

        public static async Task<Rating> ToDomain(RatingEntity entity, MusicInteractionDbContext dbContext)
        {
            // First, reconstruct the gradable component
            IGradable gradable;

            if (entity.GradableType == "grade" && entity.GradableId.HasValue)
            {
                var gradeEntity = await dbContext.Grades.FindAsync(entity.GradableId.Value);
                gradable = new Grade(
                    gradeEntity.MinGrade,
                    gradeEntity.MaxGrade,
                    gradeEntity.StepAmount,
                    gradeEntity.Name
                );

                if (gradeEntity.Grade.HasValue)
                {
                    ((Grade) gradable).updateGrade(gradeEntity.Grade.Value);
                }
            }
            else if (entity.GradableType == "method" && entity.GradableId.HasValue)
            {
                // Load and reconstruct the grading method
                gradable = await LoadGradingMethodInstance(entity.GradableId.Value, dbContext);
            }
            else if (entity.GradableType == "block" && entity.GradableId.HasValue)
            {
                // Load and reconstruct the grading block
                gradable = await LoadGradingBlock(entity.GradableId.Value, dbContext);
            }
            else
            {
                // Fallback to a basic grade if type is unknown
                gradable = new Grade();
            }

            // Create the rating with the reconstructed gradable
            var rating = new Rating(
                gradable,
                entity.AggregateId,
                entity.ItemId,
                entity.CreatedAt,
                entity.ItemType,
                entity.UserId
            );

            // Set the ID to match the original
            rating.RatingId = entity.RatingId;
            return rating;
        }

        // Helper method to store a grading method instance
        private static async Task<Guid> StoreGradingMethodInstance(GradingMethod method,
            MusicInteractionDbContext dbContext)
        {
            var componentMap = new Dictionary<string, Guid>();
            var actionsList = new List<string>();

            // Process components (recursively)
            for (int i = 0; i < method.Grades.Count; i++)
            {
                var component = method.Grades[i];
                Guid componentId;

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
                    await dbContext.SaveChangesAsync();

                    componentId = gradeEntity.EntityId;
                    componentMap["grade:" + i] = componentId;
                }
                else if (component is GradingBlock block)
                {
                    // Store the block
                    componentId = await StoreGradingBlock(block, dbContext);
                    componentMap["block:" + i] = componentId;
                }
                else
                {
                    continue;
                }

                // Add action if it's not the last component
                if (i < method.Actions.Count)
                {
                    actionsList.Add(ConvertActionToString(method.Actions[i]));
                }
            }

            // Create and save the method entity
            var methodEntity = new GradingMethodInstanceEntity
            {
                EntityId = Guid.NewGuid(),
                MethodId = method.SystemId,
                Name = method.Name,
                MinGrade = method.getMin(),
                MaxGrade = method.getMax(),
                Grade = method.getGrade(),
                NormalizedGrade = method.getNormalizedGrade()
            };

            methodEntity.Components = componentMap;
            methodEntity.Actions = actionsList;

            await dbContext.GradingMethodInstances.AddAsync(methodEntity);
            await dbContext.SaveChangesAsync();

            return methodEntity.EntityId;
        }

        // Helper method to load a grading method instance
        private static async Task<GradingMethod> LoadGradingMethodInstance(Guid entityId,
            MusicInteractionDbContext dbContext)
        {
            var methodEntity = await dbContext.GradingMethodInstances.FindAsync(entityId);
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

            // Process all components
            foreach (var componentKey in methodEntity.Components.Keys.OrderBy(k => k))
            {
                var componentId = methodEntity.Components[componentKey];
                var componentType = componentKey.Split(':')[0];

                IGradable component;

                if (componentType == "grade")
                {
                    var gradeEntity = await dbContext.Grades.FindAsync(componentId);
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
                else if (componentType == "block")
                {
                    component = await LoadGradingBlock(componentId, dbContext);
                }
                else
                {
                    continue;
                }

                method.AddGrade(component);
            }

            // Add actions
            foreach (var action in methodEntity.Actions)
            {
                method.AddAction(ConvertStringToAction(action));
            }

            return method;
        }

        // Helper method to store a grading block
        private static async Task<Guid> StoreGradingBlock(GradingBlock block, MusicInteractionDbContext dbContext)
        {
            var componentMap = new Dictionary<string, Guid>();
            var actionsList = new List<string>();

            // Process components (recursively)
            for (int i = 0; i < block.Grades.Count; i++)
            {
                var component = block.Grades[i];
                Guid componentId;

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
                    await dbContext.SaveChangesAsync();

                    componentId = gradeEntity.EntityId;
                    componentMap["grade:" + i] = componentId;
                }
                else if (component is GradingBlock nestedBlock)
                {
                    // Store the nested block
                    componentId = await StoreGradingBlock(nestedBlock, dbContext);
                    componentMap["block:" + i] = componentId;
                }
                else
                {
                    continue;
                }

                // Add action if it's not the last component
                if (i < block.Actions.Count)
                {
                    actionsList.Add(ConvertActionToString(block.Actions[i]));
                }
            }

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

            blockEntity.Components = componentMap;
            blockEntity.Actions = actionsList;

            await dbContext.GradingBlocks.AddAsync(blockEntity);
            await dbContext.SaveChangesAsync();

            return blockEntity.EntityId;
        }

        // Helper method to load a grading block
        private static async Task<GradingBlock> LoadGradingBlock(Guid entityId, MusicInteractionDbContext dbContext)
        {
            var blockEntity = await dbContext.GradingBlocks.FindAsync(entityId);
            if (blockEntity == null)
            {
                throw new Exception($"GradingBlock with ID {entityId} not found");
            }

            // Create a new instance of GradingBlock
            var block = new GradingBlock(blockEntity.Name);

            // Clear default components and actions
            block.Grades.Clear();
            block.Actions.Clear();

            // Process all components
            foreach (var componentKey in blockEntity.Components.Keys.OrderBy(k => k))
            {
                var componentId = blockEntity.Components[componentKey];
                var componentType = componentKey.Split(':')[0];

                IGradable component;

                if (componentType == "grade")
                {
                    var gradeEntity = await dbContext.Grades.FindAsync(componentId);
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
                else if (componentType == "block")
                {
                    component = await LoadGradingBlock(componentId, dbContext);
                }
                else
                {
                    continue;
                }

                block.AddGrade(component);
            }

            // Add actions
            foreach (var action in blockEntity.Actions)
            {
                block.AddAction(ConvertStringToAction(action));
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