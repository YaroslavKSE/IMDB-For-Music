namespace MusicInteraction.Application;

public class BlockDetailShowDto : GradableComponentShowDto
{
    public List<GradableComponentShowDto> Components { get; set; }
    public List<string> Actions { get; set; }
}