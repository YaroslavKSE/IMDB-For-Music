namespace MusicInteraction.Application;

public class BlockDetailShowDto : GradableComponentShowDto
{
    public List<GradableComponentShowDto> SubComponents { get; set; }
    public List<string> Actions { get; set; }
}