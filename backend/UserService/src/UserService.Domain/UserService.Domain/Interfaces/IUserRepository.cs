using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<User> GetByAuth0IdAsync(string auth0Id);
    Task<(List<User> Users, int TotalCount)> GetPaginatedUsersAsync(int page, int pageSize, string searchTerm = null, CancellationToken cancellationToken = default);

}