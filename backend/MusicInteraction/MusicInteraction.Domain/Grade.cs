namespace MusicInteraction.Domain;

public class Grade: IGradable
{
    public string parametrName { get; private set; }
    public float minGrade { get; private set; }
    public float maxGrade { get; private set; }
    public float stepAmount { get; private set; }
    public float? grade { get; private set; }

    public Grade(float minGrade = 1, float maxGrade = 10, float stepAmount = 1, string parametrName = "basicRating")
    {
        this.minGrade = minGrade;
        this.maxGrade = maxGrade;
        this.stepAmount = stepAmount;
        this.parametrName = parametrName;
        this.grade = null;
    }

    public void updateGrade(float grade)
    {
        for (float i = minGrade; i <= maxGrade; i += stepAmount)
        {
            if (grade == i)
               return;
        }
        throw new Exception("unvalid grade");
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