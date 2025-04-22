using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class DeleteInteractionUseCase : IRequestHandler<DeleteInteractionCommand, DeleteInteractionResult>
{
    private readonly IInteractionStorage _interactionStorage;
    private readonly IItemStatsStorage _itemStatsStorage;

    public DeleteInteractionUseCase(
        IInteractionStorage interactionStorage,
        IItemStatsStorage itemStatsStorage)
    {
        _interactionStorage = interactionStorage;
        _itemStatsStorage = itemStatsStorage;
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

            // Save the itemId before deleting the interaction
            string itemId = interaction.ItemId;

            // Delete the interaction
            await _interactionStorage.DeleteInteractionAsync(request.InteractionId);

            // Mark item stats as raw to be recalculated
            bool statsExists = await _itemStatsStorage.ItemStatsExistsAsync(itemId);
            if (statsExists)
            {
                await _itemStatsStorage.MarkItemStatsAsRawAsync(itemId);
            }

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