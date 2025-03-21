using MediatR;

namespace MusicInteraction.Application;

public class GetGradingMethodByIdCommand : IRequest<GetGradingMethodDetailResult>
{
    public Guid GradingMethodId { get; set; }
}