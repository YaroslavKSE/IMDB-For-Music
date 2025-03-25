using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetGradingMethodsByCreatorIdUseCase : IRequestHandler<GetGradingMethodsByCreatorIdCommand, GetGradingMethodsResult>
{
    private readonly IGradingMethodStorage gradingMethodStorage;

    public GetGradingMethodsByCreatorIdUseCase(IGradingMethodStorage gradingMethodStorage)
    {
        this.gradingMethodStorage = gradingMethodStorage;
    }

    public async Task<GetGradingMethodsResult> Handle(GetGradingMethodsByCreatorIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = new GetGradingMethodsResult
            {
                GradingMethods = new List<GradingMethodSummaryDto>(),
                Success = true
            };

            var publicMethods = await gradingMethodStorage.GetUserGradingMethods(request.CreatorId);

            // Convert to DTOs with summary information
            foreach (var method in publicMethods)
            {
                result.GradingMethods.Add(new GradingMethodSummaryDto
                {
                    Id = method.SystemId,
                    Name = method.Name,
                    CreatorId = method.CreatorId,
                    CreatedAt = method.CreatedAt,
                    IsPublic = method.IsPublic
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            return new GetGradingMethodsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                GradingMethods = new List<GradingMethodSummaryDto>()
            };
        }
    }
}