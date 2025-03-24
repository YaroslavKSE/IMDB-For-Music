using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class UpdateGradingMethodUseCase : IRequestHandler<UpdateGradingMethodCommand, CreateGradingMethodResult>
{
    private readonly IGradingMethodStorage _gradingMethodStorage;

    public UpdateGradingMethodUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        _gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<CreateGradingMethodResult> Handle(UpdateGradingMethodCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _gradingMethodStorage.IsEmpty())
            {
                return new CreateGradingMethodResult
                {
                    Success = false,
                    ErrorMessage = "No grading methods found"
                };
            }

            var gradingMethodToUpdate = await _gradingMethodStorage.GetGradingMethodById(request.GradingMethodId);

            var updatedGradingMethod = new GradingMethod(gradingMethodToUpdate.SystemId, gradingMethodToUpdate.CreatedAt, gradingMethodToUpdate.Name, gradingMethodToUpdate.CreatorId, request.IsPublic);

            GradingMethodBuilder.BuildGradingMethod(request.Components, request.Actions, updatedGradingMethod);

            await _gradingMethodStorage.UpdateGradingMethodAsync(updatedGradingMethod);

            return new CreateGradingMethodResult
            {
                Success = true,
                GradingMethodId = updatedGradingMethod.SystemId
            };
        }
        catch (Exception ex)
        {
            return new CreateGradingMethodResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}