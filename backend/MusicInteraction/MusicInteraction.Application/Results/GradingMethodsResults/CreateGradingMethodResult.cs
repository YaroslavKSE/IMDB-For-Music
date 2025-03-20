namespace MusicInteraction.Application;

public class CreateGradingMethodResult
{
    public bool Success { get; set; }
    public Guid? GradingMethodId { get; set; }
    public string ErrorMessage { get; set; }
}