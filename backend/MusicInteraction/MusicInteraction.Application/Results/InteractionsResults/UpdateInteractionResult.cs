namespace MusicInteraction.Application;

public class UpdateInteractionResult
{
    public bool InteractionUpdated { get; set; }
    public bool LikeUpdated { get; set; }
    public bool ReviewUpdated { get; set; }
    public bool GradingUpdated { get; set; }
    public Guid InteractionId { get; set; }
    public string ErrorMessage { get; set; }
}