namespace UserService.API.Models.Responses;

public class PresignedUrlResponse
{
    public string Url { get; set; }
    public Dictionary<string, string> FormData { get; set; }
    public string ObjectKey { get; set; }
    public string AvatarUrl { get; set; }
    public int ExpiresInSeconds { get; set; }
}