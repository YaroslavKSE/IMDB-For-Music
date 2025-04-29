namespace UserService.Domain.Entities;

public class UserPreference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public PreferenceType ItemType { get; private set; }
    public string SpotifyId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public virtual User User { get; private set; }

    private UserPreference()
    {
    } // For EF Core

    // Factory method
    public static UserPreference Create(Guid userId, PreferenceType itemType, string spotifyId)
    {
        return new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ItemType = itemType,
            SpotifyId = spotifyId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum PreferenceType
{
    Artist,
    Album,
    Track
}