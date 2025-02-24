using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class WriteReviewCommand: IRequest<WriteReviewResult>
{
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ReviewText { get; set; }
}

public class WriteReviewResult
{
    public bool ReviewCreated;
}

public class WriteReviewRequest
{
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ReviewText { get; set; }
}


public class WriteReviewUseCase : IRequestHandler<WriteReviewCommand, WriteReviewResult>
{
    private readonly IInteractionStorage InteractionStorage;

    public WriteReviewUseCase(IInteractionStorage _interactionStorage)
    {
        InteractionStorage = _interactionStorage;
    }

    public async Task<WriteReviewResult> Handle(WriteReviewCommand request, CancellationToken cancellationToken)
    {
        bool reviewCreated = await InteractionStorage.AddReview(request.UserId, request.ItemId, request.ReviewText);
        return new WriteReviewResult() {ReviewCreated = reviewCreated};
    }
}