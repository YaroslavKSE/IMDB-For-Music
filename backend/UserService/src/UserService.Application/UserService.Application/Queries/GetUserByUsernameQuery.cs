﻿using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public class GetUserByUsernameQuery : IRequest<UserResponse>
{
    public string Username { get; }

    public GetUserByUsernameQuery(string username)
    {
        Username = username;
    }
}