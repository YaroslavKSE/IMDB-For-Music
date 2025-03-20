using Microsoft.EntityFrameworkCore;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;

public static class GradeMapper
{
    public static async Task<GradeEntity> MapToEntityAsync(Grade grade, MusicInteractionDbContext dbContext, Guid? ratingId = null)
    {
        var gradeEntity = new GradeEntity
        {
            EntityId = Guid.NewGuid(),
            Name = grade.parametrName,
            MinGrade = grade.getMin(),
            MaxGrade = grade.getMax(),
            Grade = grade.getGrade(),
            StepAmount = grade.stepAmount,
            NormalizedGrade = grade.getNormalizedGrade(),
            RatingId = ratingId
        };

        await dbContext.Grades.AddAsync(gradeEntity);
        return gradeEntity;
    }

    public static async Task<Grade> GetByRatingIdAsync(Guid ratingId, MusicInteractionDbContext dbContext)
    {
        var gradeEntity = await dbContext.Grades
            .FirstOrDefaultAsync(g => g.RatingId == ratingId);

        if (gradeEntity == null)
        {
            return new Grade(); // Return a default grade if not found
        }

        return CreateGradeFromEntity(gradeEntity);
    }

    public static async Task<Grade> GetByEntityIdAsync(Guid entityId, MusicInteractionDbContext dbContext)
    {
        var gradeEntity = await dbContext.Grades
            .FirstOrDefaultAsync(g => g.EntityId == entityId);

        if (gradeEntity == null)
        {
            return new Grade(); // Return a default grade if not found
        }

        return CreateGradeFromEntity(gradeEntity);
    }

    private static Grade CreateGradeFromEntity(GradeEntity gradeEntity)
    {
        var grade = new Grade(
            gradeEntity.MinGrade,
            gradeEntity.MaxGrade,
            gradeEntity.StepAmount,
            gradeEntity.Name
        );

        if (gradeEntity.Grade.HasValue)
        {
            try
            {
                grade.updateGrade(gradeEntity.Grade.Value);
            }
            catch (Exception ex)
            {
                // Log error but continue with the grade without the value
                Console.WriteLine($"Error setting grade value: {ex.Message}");
            }
        }

        return grade;
    }
}