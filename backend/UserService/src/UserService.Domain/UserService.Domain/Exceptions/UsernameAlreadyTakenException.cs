namespace UserService.Domain.Exceptions;

public class UsernameAlreadyTakenException : Exception
{
    public string Username { get; }

    public UsernameAlreadyTakenException(string username)
        : base($"Username '{username}' is already taken.")
    {
        Username = username;
    }

    public UsernameAlreadyTakenException(string username, Exception innerException)
        : base($"Username '{username}' is already taken.", innerException)
    {
        Username = username;
    }
}