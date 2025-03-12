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
}