using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
namespace MusicInteraction.Application;

public class GetReviewsCommand: IRequest<GetReviewsResult> { }

public class GetReviewsResult
{
    public bool ReviewsEmpty { get; set; }
    public List<Review> Reviews { get; set; }
}

public class GetReviewsUseCase : IRequestHandler<GetReviewsCommand, GetReviewsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetReviewsUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetReviewsResult> Handle(GetReviewsCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetReviewsResult() {ReviewsEmpty = true};
        }
        return new GetReviewsResult() {ReviewsEmpty = false, Reviews = interactionStorage.GetReviews().Result};
    }
}