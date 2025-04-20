using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class RemoveReviewLikeUseCase : IRequestHandler<RemoveReviewLikeCommand, RemoveReviewLikeResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public RemoveReviewLikeUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<RemoveReviewLikeResult> Handle(RemoveReviewLikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _interactionStorage.RemoveReviewLike(request.ReviewId, request.UserId);

            return new RemoveReviewLikeResult
            {
                Success = result,
                ErrorMessage = result ? null : "Review like not found"
            };
        }
        catch (Exception ex)
        {
            return new RemoveReviewLikeResult
            {
                Success = false,
                ErrorMessage = $"Error removing review like: {ex.Message}"
            };
        }
    }
}