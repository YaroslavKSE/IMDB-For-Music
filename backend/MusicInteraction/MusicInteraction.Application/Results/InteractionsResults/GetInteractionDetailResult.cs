namespace MusicInteraction.Application;

public class GetInteractionDetailResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public InteractionAggregateShowDto Interaction { get; set; }
}