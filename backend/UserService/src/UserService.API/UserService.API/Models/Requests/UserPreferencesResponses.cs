namespace UserService.API.Models.Requests;

public class UserPreferencesResponse
{
    public List<string> Artists { get; set; } = new();
    public List<string> Albums { get; set; } = new();
    public List<string> Tracks { get; set; } = new();
}

public class PreferenceOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}