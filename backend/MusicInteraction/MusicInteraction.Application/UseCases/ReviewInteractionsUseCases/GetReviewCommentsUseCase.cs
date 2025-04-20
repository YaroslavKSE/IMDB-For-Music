using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetReviewCommentsUseCase : IRequestHandler<GetReviewCommentsCommand, GetReviewCommentsResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public GetReviewCommentsUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<GetReviewCommentsResult> Handle(GetReviewCommentsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedResult = await _interactionStorage.GetReviewComments(request.ReviewId, request.Limit, request.Offset);

            var commentDtos = paginatedResult.Items.Select(c => new ReviewCommentDTO
            {
                CommentId = c.CommentId,
                ReviewId = c.ReviewId,
                UserId = c.UserId,
                CommentedAt = c.CommentedAt,
                CommentText = c.CommentText
            }).ToList();

            return new GetReviewCommentsResult
            {
                Success = true,
                Comments = commentDtos,
                TotalCount = paginatedResult.TotalCount
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new GetReviewCommentsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Comments = new List<ReviewCommentDTO>()
            };
        }
        catch (Exception ex)
        {
            return new GetReviewCommentsResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving review comments: {ex.Message}",
                Comments = new List<ReviewCommentDTO>()
            };
        }
    }
}