using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetInteractionsCommand: IRequest<GetInteractionsResult>
{

}

public class GetInteractionsResult
{
    public bool InteractionsEmpty { get; set; }
    public List<InteractionsAggregate> Interactions { get; set; }
}

public class GetInteractionsUseCase : IRequestHandler<GetInteractionsCommand, GetInteractionsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetInteractionsUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionsResult> Handle(GetInteractionsCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetInteractionsResult() {InteractionsEmpty = true};
        }
        return new GetInteractionsResult() {InteractionsEmpty = false, Interactions = interactionStorage.GetInteractions().Result};
    }
}
