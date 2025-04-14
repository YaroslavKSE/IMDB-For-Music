namespace UserService.Infrastructure.Configuration;

public class AWSSettings
{
    public string AvatarBucketName { get; set; }
    public string AvatarBaseUrl { get; set; }
    public string Region { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}