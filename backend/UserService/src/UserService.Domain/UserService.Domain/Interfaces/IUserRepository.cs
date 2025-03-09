using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<User> GetByAuth0IdAsync(string auth0Id);
}