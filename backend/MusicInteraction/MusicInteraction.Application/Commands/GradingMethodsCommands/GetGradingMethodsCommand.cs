using MediatR;

namespace MusicInteraction.Application;

public class GetPublicGradingMethodsCommand : IRequest<GetGradingMethodsResult> { }