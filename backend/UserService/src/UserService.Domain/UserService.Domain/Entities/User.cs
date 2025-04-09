namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; } 
    public string Name { get; private set; }
    public string Surname { get; private set; }
    public string Auth0Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { } // For EF Core

    public static User Create(string email, string username, string name, string surname, string auth0Id)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            Name = name,
            Surname = surname,
            Auth0Id = auth0Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string username, string name, string surname)
    {
        Username = username;
        Name = name;
        Surname = surname;
        UpdatedAt = DateTime.UtcNow;
    }
}
