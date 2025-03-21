using MusicInteraction.Domain;

namespace MusicInteraction.Infrastructure.PostgreSQL.Mapping;


public static class ActionMapper
{
    public static string ConvertActionToString(Domain.Action action)
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

    public static Domain.Action ConvertStringToAction(string actionStr)
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