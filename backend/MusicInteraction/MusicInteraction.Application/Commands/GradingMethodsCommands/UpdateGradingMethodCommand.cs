using MediatR;

namespace MusicInteraction.Application;

public class UpdateGradingMethodCommand: IRequest<CreateGradingMethodResult>
{
    public Guid GradingMethodId { get; set; }
    public bool IsPublic { get; set; }
    public List<ComponentDto> Components { get; set; }
    public List<Domain.Action> Actions { get; set; }
}