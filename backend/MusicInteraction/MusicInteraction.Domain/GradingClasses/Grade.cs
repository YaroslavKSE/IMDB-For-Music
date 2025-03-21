namespace MusicInteraction.Domain;

public class Grade: IGradable
{
    public string parametrName { get; private set; }
    public float minGrade { get; private set; }
    public float maxGrade { get; private set; }
    public float stepAmount { get; private set; }
    public float? grade { get; private set; }
    public string? Description { get; private set; }

    public Grade(float minGrade = 1, float maxGrade = 10, float stepAmount = 1, string parametrName = "basicRating", string? description = null)
    {
        this.minGrade = minGrade;
        this.maxGrade = maxGrade;
        this.stepAmount = stepAmount;
        this.parametrName = parametrName;
        this.Description = description;
        this.grade = null;
    }

    public void updateGrade(float grade)
    {
        // Round to nearest valid step to avoid floating-point precision issues
        float roundedGrade = (float)Math.Round(grade / stepAmount) * stepAmount;

        // Ensure the rounded grade is within range
        if (roundedGrade >= minGrade && roundedGrade <= maxGrade)
        {
            this.grade = roundedGrade;
            return;
        }

        throw new Exception($"Invalid grade: {grade}. Must be between {minGrade} and {maxGrade} with steps of {stepAmount}");
    }

    public float? getGrade()
    {
        return grade;
    }

    public float getMax()
    {
        return maxGrade;
    }

    public float getMin()
    {
        return minGrade;
    }

    public string getName()
    {
        return parametrName;
    }

    public float? getNormalizedGrade()
    {
        if (grade == null)
            return null;

        // Calculate what percentage of the possible range the current grade represents
        float range = maxGrade - minGrade;
        float normalizedPercentage = (grade.Value - minGrade) / range;

        // Scale this percentage to the 1-10 range with step amount of 1
        float normalizedValue = 1 + normalizedPercentage * 9; // 9 because the range is from 1 to 10 (9 intervals)

        // Round to nearest integer to ensure step amount of 1
        return (float)Math.Round(normalizedValue);
    }
}