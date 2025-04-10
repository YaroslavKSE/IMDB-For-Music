using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.QueryHandlers;

public class GetUserProfileQueryHandler : BaseUserQueryHandler, IRequestHandler<GetUserProfileQuery, UserResponse>
{
    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserProfileQueryHandler> logger)
        : base(userRepository, logger)
    {
    }

    public async Task<UserResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return await GetUserByAuth0IdAsync(request.Auth0UserId);
    }
}