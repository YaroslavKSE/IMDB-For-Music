namespace MusicInteraction.Application;

public class BlockComponentDto : ComponentDto
{
    public List<ComponentDto> SubComponents { get; set; }
    public List<Domain.Action> Actions { get; set; }
}