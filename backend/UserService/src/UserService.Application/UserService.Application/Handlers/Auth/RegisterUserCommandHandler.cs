using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IAuth0Service auth0Service,
        ILogger<RegisterUserCommandHandler> logger,
        IValidator<RegisterUserCommand> validator)
    {
        _userRepository = userRepository;
        _auth0Service = auth0Service;
        _logger = logger;
        _validator = validator;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        // Check if user exists by email
        var existingUserByEmail = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUserByEmail != null) throw new UserAlreadyExistsException(command.Email);

        // Check if username is already taken
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(command.Username);
        if (existingUserByUsername != null) throw new UsernameAlreadyTakenException(command.Username);

        // Create user in Auth0
        var auth0Id = await _auth0Service.CreateUserAsync(command.Email, command.Password);

        // Create user in our database
        var user = User.Create(command.Email, command.Username, command.Name, command.Surname, auth0Id);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered successfully with username {Username}", command.Email,
            command.Username);

        return new RegisterUserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            Name = user.Name,
            Surname = user.Surname,
            CreatedAt = user.CreatedAt
        };
    }
}