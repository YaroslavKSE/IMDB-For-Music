﻿using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public class GetUserByEmailQuery : IRequest<UserResponse>
{
    public string Email { get; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}