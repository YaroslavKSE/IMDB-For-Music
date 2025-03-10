using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
namespace MusicInteraction.Application;

public class GetRatingsCommand: IRequest<GetRatingsResult> { }

public class GetRatingsResult
{
    public bool RatingsEmpty { get; set; }
    public List<Rating> Ratings { get; set; }
}

public class GetRatingsUseCase : IRequestHandler<GetRatingsCommand, GetRatingsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetRatingsUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetRatingsResult> Handle(GetRatingsCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetRatingsResult() {RatingsEmpty = true};
        }
        return new GetRatingsResult() {RatingsEmpty = false, Ratings = interactionStorage.GetRatings().Result};
    }
}