namespace MusicInteraction.Domain;

public class Grade: IGradable
{
    private string parametrName;
    private float minGrade;
    private float maxGrade;
    private float stepAmount;
    private float? grade;

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
}