namespace UserService.API.Models.Requests;

public class CompleteAvatarUploadRequest
{
    public string ObjectKey { get; set; }
    public string AvatarUrl { get; set; }
}