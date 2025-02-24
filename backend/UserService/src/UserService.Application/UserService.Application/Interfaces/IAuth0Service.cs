using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IAuth0Service
{
    Task<string> CreateUserAsync(string email, string password);
    Task<AuthTokenResponse> LoginAsync(string email, string password);
}