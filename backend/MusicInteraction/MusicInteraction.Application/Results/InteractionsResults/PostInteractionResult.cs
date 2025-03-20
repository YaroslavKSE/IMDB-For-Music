public class PostInteractionResult
{
    public bool InteractionCreated { get; set; }
    public bool Liked { get; set; }
    public bool ReviewCreated { get; set; }
    public bool Graded { get; set; }
    public Guid InteractionId { get; set; }
    public string ErrorMessage { get; set; }
}