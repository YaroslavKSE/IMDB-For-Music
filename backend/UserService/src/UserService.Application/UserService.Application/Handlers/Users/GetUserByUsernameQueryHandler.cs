﻿using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries.Users;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Users;

public class GetUserByUsernameQueryHandler : BaseUserQueryHandler, IRequestHandler<GetUserByUsernameQuery, UserResponse>
{
    public GetUserByUsernameQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByUsernameQueryHandler> logger)
        : base(userRepository, logger)
    {
    }

    public async Task<UserResponse> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        return await GetUserByUsernameAsync(request.Username);
    }
}