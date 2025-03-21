namespace MusicInteraction.Application;

public class GetGradingMethodsResult
{
    public bool Success { get; set; }
    public List<GradingMethodSummaryDto> GradingMethods { get; set; }
    public string ErrorMessage { get; set; }
}