using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class DeleteGradingMethodUseCase : IRequestHandler<DeleteGradingMethodCommand, DeleteGradingMethodResult>
{
    private readonly IGradingMethodStorage _gradingMethodStorage;

    public DeleteGradingMethodUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        _gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<DeleteGradingMethodResult> Handle(DeleteGradingMethodCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _gradingMethodStorage.IsEmpty())
            {
                return new DeleteGradingMethodResult
                {
                    Success = false,
                    ErrorMessage = "No grading methods found"
                };
            }

            var gradingMethod = await _gradingMethodStorage.GetGradingMethodById(request.GradingMethodId);
            if (gradingMethod == null)
            {
                return new DeleteGradingMethodResult
                {
                    Success = false,
                    ErrorMessage = $"Grading method with ID {request.GradingMethodId} not found"
                };
            }

            await _gradingMethodStorage.DeleteGradingMethodAsync(request.GradingMethodId);

            return new DeleteGradingMethodResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new DeleteGradingMethodResult()
            {
                Success = false,
                ErrorMessage = $"Error deleting gradingMethod: {ex.Message}"
            };
        }
    }
}