namespace MusicInteraction.Application;

public class GradedBlockDetailDTO : GradedComponentDTO
{
    public List<GradedComponentDTO> Components { get; set; }
    public List<string> Actions { get; set; }
}