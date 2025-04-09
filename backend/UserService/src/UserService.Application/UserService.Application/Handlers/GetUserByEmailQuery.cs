using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class GetUserByEmailQueryHandler : BaseUserQueryHandler, IRequestHandler<GetUserByEmailQuery, UserResponse>
{
    public GetUserByEmailQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByEmailQueryHandler> logger)
        : base(userRepository, logger)
    {
    }

    public async Task<UserResponse> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        return await GetUserByEmailAsync(request.Email);
    }
}