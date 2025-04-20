using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class DeleteReviewCommentUseCase : IRequestHandler<DeleteReviewCommentCommand, DeleteReviewCommentResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public DeleteReviewCommentUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<DeleteReviewCommentResult> Handle(DeleteReviewCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _interactionStorage.DeleteReviewComment(request.CommentId, request.UserId);

            return new DeleteReviewCommentResult
            {
                Success = result,
                ErrorMessage = result ? null : "Comment not found or you don't have permission to delete it"
            };
        }
        catch (Exception ex)
        {
            return new DeleteReviewCommentResult
            {
                Success = false,
                ErrorMessage = $"Error deleting review comment: {ex.Message}"
            };
        }
    }
}