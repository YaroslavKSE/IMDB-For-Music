namespace MusicInteraction.Application;

public class GetGradingMethodDetailResult
{
    public bool Success { get; set; }
    public GradingMethodDetailDto GradingMethod { get; set; }
    public string ErrorMessage { get; set; }
}