using System.Collections.ObjectModel;

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
    public string AvatarUrl { get; private set; }

    private readonly List<UserSubscription> _followers = new();
    private readonly List<UserSubscription> _following = new();

    public virtual IReadOnlyCollection<UserSubscription> Followers =>
        new ReadOnlyCollection<UserSubscription>(_followers);

    public virtual IReadOnlyCollection<UserSubscription> Following =>
        new ReadOnlyCollection<UserSubscription>(_following);

    private User()
    {
    } // For EF Core

    public static User Create(string email, string username, string name, string surname, string auth0Id, string avatarUrl = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            Name = name,
            Surname = surname,
            Auth0Id = auth0Id,
            AvatarUrl = avatarUrl,
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
    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}