using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class CheckUserLikedReviewUseCase : IRequestHandler<CheckUserLikedReviewCommand, CheckUserLikedReviewResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public CheckUserLikedReviewUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<CheckUserLikedReviewResult> Handle(CheckUserLikedReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool hasLiked = await _interactionStorage.HasUserLikedReview(request.ReviewId, request.UserId);

            return new CheckUserLikedReviewResult
            {
                Success = true,
                HasLiked = hasLiked
            };
        }
        catch (Exception ex)
        {
            return new CheckUserLikedReviewResult
            {
                Success = false,
                ErrorMessage = $"Error checking if user liked review: {ex.Message}"
            };
        }
    }
}