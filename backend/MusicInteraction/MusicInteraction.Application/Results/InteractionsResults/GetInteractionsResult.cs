namespace MusicInteraction.Application;

public class GetInteractionsResult
{
    public bool InteractionsEmpty { get; set; }
    public List<InteractionAggregateShowDto> Interactions { get; set; }
    public int TotalCount { get; set; }
}