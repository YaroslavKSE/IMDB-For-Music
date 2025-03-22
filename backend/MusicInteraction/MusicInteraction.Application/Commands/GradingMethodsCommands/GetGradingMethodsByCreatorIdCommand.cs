using MediatR;

namespace MusicInteraction.Application;

public class GetGradingMethodsByCreatorIdCommand : IRequest<GetGradingMethodsResult>
{
    public string CreatorId { get; set; }
}