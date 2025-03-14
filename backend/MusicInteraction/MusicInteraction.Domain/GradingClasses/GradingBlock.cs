namespace MusicInteraction.Domain;

public class GradingBlock : IGradable
{
    public string BlockName { get; private set; }
    public List<IGradable> Grades { get; private set; }
    public List<Action> Actions { get; private set; }

    public GradingBlock(string blockName)
    {
        BlockName = blockName;
        Grades = new List<IGradable>();
        Actions = new List<Action>();
    }

    public void AddGrade(IGradable grade)
    {
        Grades.Add(grade);
    }

    public void AddAction(Action action)
    {
        Actions.Add(action);
    }

    public float? getGrade()
    {
        if (Grades.Count == 0)
            return null;

        if (Grades.Count == 1)
            return Grades[0].getGrade();

        float? result = Grades[0].getGrade();

        for (int i = 1; i < Grades.Count; i++)
        {
            if (Grades[i].getGrade() == null)
                continue;

            switch (Actions[i-1])
            {
                case Action.Add:
                    result += Grades[i].getGrade();
                    break;
                case Action.Subtract:
                    result -= Grades[i].getGrade();
                    break;
                case Action.Multiply:
                    result *= Grades[i].getGrade();
                    break;
                case Action.Divide:
                    if (Grades[i].getGrade() != 0)
                        result /= Grades[i].getGrade();
                    break;
            }
        }

        return result;
    }

    public float getMax()
    {
        if (Grades.Count == 0)
            throw new Exception("no grading parametrs were added");

        if (Grades.Count == 1)
            return Grades[0].getMax();

        float max = Grades[0].getMax();

        for (int i = 1; i < Grades.Count; i++)
        {
            switch (Actions[i-1])
            {
                case Action.Add:
                    max += Grades[i].getMax();
                    break;
                case Action.Subtract:
                    max -= Grades[i].getMin();
                    break;
                case Action.Multiply:
                    max *= Grades[i].getMax();
                    break;
                case Action.Divide:
                    float minValue = Grades[i].getMin();
                    if (minValue > 0)
                        max /= minValue;
                    break;
            }
        }

        return max;
    }

    public float getMin()
    {
        if (Grades.Count == 0)
        {
            throw new Exception("no grading parametrs were added");
        }
        else if (Grades.Count == 1)
        {
            return Grades[0].getMin();
        }

        float min = Grades[0].getMin();

        for (int i = 1; i < Grades.Count; i++)
        {
            switch (Actions[i-1])
            {
                case Action.Add:
                    min += Grades[i].getMin();
                    break;
                case Action.Subtract:
                    min -= Grades[i].getMax();
                    break;
                case Action.Multiply:
                    min *= Grades[i].getMin();
                    break;
                case Action.Divide:
                    float maxValue = Grades[i].getMax();
                    if (maxValue != 0)
                        min /= maxValue;
                    break;
            }
        }

        return min;
    }

    public float? getNormalizedGrade()
    {
        float? currentGrade = getGrade();
        if (currentGrade == null)
            return null;

        float min = getMin();
        float max = getMax();
        float range = max - min;

        float normalizedPercentage = (currentGrade.Value - min) / range;

        float normalizedValue = 1 + normalizedPercentage * 9;

        return (float)Math.Round(normalizedValue);
    }
}