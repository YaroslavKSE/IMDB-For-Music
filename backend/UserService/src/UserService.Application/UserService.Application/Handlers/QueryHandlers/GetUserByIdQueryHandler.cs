using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.QueryHandlers;

public class GetUserByIdQueryHandler : BaseUserQueryHandler, IRequestHandler<GetUserByIdQuery, UserResponse>
{
    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByIdQueryHandler> logger)
        : base(userRepository, logger)
    {
    }

    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await GetUserByIdAsync(request.UserId);
    }
}