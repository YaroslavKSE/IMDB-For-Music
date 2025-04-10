using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> GetByAuth0IdAsync(string auth0Id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(List<User> Users, int TotalCount)> GetPaginatedUsersAsync(int page, int pageSize,
        string searchTerm = null, CancellationToken cancellationToken = default)
    {
        // Start with the base query
        IQueryable<User> query = _context.Users;

        // Apply search if search term is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower().Trim();
            query = query.Where(u =>
                u.Username.ToLower().Contains(searchTerm) ||
                u.Name.ToLower().Contains(searchTerm) ||
                u.Surname.ToLower().Contains(searchTerm));
        }

        // Get total count for pagination info
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to include only necessary properties
        var users = await query
            .OrderBy(u => u.Username) // Default ordering by username
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}