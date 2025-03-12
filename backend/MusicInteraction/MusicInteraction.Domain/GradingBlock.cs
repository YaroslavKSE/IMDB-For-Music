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

    public void AddGradeWithAction(IGradable grade, Action action)
    {
        Grades.Add(grade);
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
        //TODO
        // For simplicity, I return the max of the first grade
        return Grades.Count > 0 ? Grades[0].getMax() : 10f;
    }
}