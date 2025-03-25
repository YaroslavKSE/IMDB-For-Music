using MediatR;

namespace MusicInteraction.Application;

public class DeleteGradingMethodCommand : IRequest<DeleteGradingMethodResult>
{
    public Guid GradingMethodId { get; set; }
}