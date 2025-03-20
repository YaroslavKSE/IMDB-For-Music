using MediatR;

namespace MusicInteraction.Application;

public class CreateGradingMethodCommand : IRequest<CreateGradingMethodResult>
{
    public string Name { get; set; }
    public string UserId { get; set; }
    public bool IsPublic { get; set; }
    public List<ComponentDto> Components { get; set; }
    public List<Domain.Action> Actions { get; set; }
}