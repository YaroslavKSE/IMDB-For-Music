using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class DeleteInteractionUseCase : IRequestHandler<DeleteInteractionCommand, DeleteInteractionResult>
{
    private readonly IInteractionStorage _interactionStorage;

    public DeleteInteractionUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<DeleteInteractionResult> Handle(DeleteInteractionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _interactionStorage.IsEmpty())
            {
                return new DeleteInteractionResult
                {
                    Success = false,
                    ErrorMessage = "No interactions found"
                };
            }

            // Check if the interaction exists
            var interaction = await _interactionStorage.GetInteractionById(request.InteractionId);
            if (interaction == null)
            {
                return new DeleteInteractionResult
                {
                    Success = false,
                    ErrorMessage = $"Interaction with ID {request.InteractionId} not found"
                };
            }

            // Delete the interaction
            await _interactionStorage.DeleteInteractionAsync(request.InteractionId);

            return new DeleteInteractionResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new DeleteInteractionResult
            {
                Success = false,
                ErrorMessage = $"Error deleting interaction: {ex.Message}"
            };
        }
    }
}