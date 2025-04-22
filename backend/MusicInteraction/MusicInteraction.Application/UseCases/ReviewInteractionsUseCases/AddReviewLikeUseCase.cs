using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class AddReviewLikeUseCase : IRequestHandler<AddReviewLikeCommand, AddReviewLikeResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public AddReviewLikeUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<AddReviewLikeResult> Handle(AddReviewLikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reviewLike = await _interactionStorage.AddReviewLike(request.ReviewId, request.UserId);

            return new AddReviewLikeResult
            {
                Success = true,
                Like = new ReviewLikeDTO
                {
                    LikeId = reviewLike.LikeId,
                    ReviewId = reviewLike.ReviewId,
                    UserId = reviewLike.UserId,
                    LikedAt = reviewLike.LikedAt
                }
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new AddReviewLikeResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new AddReviewLikeResult
            {
                Success = false,
                ErrorMessage = $"Error adding review like: {ex.Message}"
            };
        }
    }
}