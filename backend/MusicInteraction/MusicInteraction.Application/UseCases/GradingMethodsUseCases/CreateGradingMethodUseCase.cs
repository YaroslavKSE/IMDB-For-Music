using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class CreateGradingMethodUseCase : IRequestHandler<CreateGradingMethodCommand, CreateGradingMethodResult>
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public CreateGradingMethodUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        this.gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<CreateGradingMethodResult> Handle(CreateGradingMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var gradingMethod = new GradingMethod(request.Name, request.UserId, request.IsPublic);

            GradingMethodBuilder.BuildGradingMethod(request.Components, request.Actions, gradingMethod);

            await gradingMethodStorage.AddGradingMethodAsync(gradingMethod);

            return new CreateGradingMethodResult
            {
                Success = true,
                GradingMethodId = gradingMethod.SystemId
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