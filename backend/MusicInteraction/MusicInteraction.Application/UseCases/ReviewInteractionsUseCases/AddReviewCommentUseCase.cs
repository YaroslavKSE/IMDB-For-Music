using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class AddReviewCommentUseCase : IRequestHandler<AddReviewCommentCommand, AddReviewCommentResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public AddReviewCommentUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<AddReviewCommentResult> Handle(AddReviewCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CommentText))
            {
                return new AddReviewCommentResult
                {
                    Success = false,
                    ErrorMessage = "Comment text cannot be empty"
                };
            }

            var comment = await _interactionStorage.AddReviewComment(request.ReviewId, request.UserId, request.CommentText);

            return new AddReviewCommentResult
            {
                Success = true,
                Comment = new ReviewCommentDTO
                {
                    CommentId = comment.CommentId,
                    ReviewId = comment.ReviewId,
                    UserId = comment.UserId,
                    CommentedAt = comment.CommentedAt,
                    CommentText = comment.CommentText
                }
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new AddReviewCommentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new AddReviewCommentResult
            {
                Success = false,
                ErrorMessage = $"Error adding review comment: {ex.Message}"
            };
        }
    }
}