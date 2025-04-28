using Microsoft.AspNetCore.Http;

namespace UserService.Application.Interfaces;

public interface IS3StorageService
{
    /// <summary>
    /// Uploads a file to S3 storage
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="userId">The user ID to associate with the file</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadUserAvatarAsync(IFormFile file, Guid userId);

    /// <summary>
    /// Deletes a user's avatar file from S3 storage
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteUserAvatarAsync(Guid userId);

    /// <summary>
    /// Generates a presigned URL for direct browser uploads
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="contentType">The file content type</param>
    /// <returns>Presigned URL and form data if needed</returns>
    Task<PresignedUploadResponse> GeneratePresignedUploadUrlAsync(Guid userId, string contentType);
}

public class PresignedUploadResponse
{
    public string Url { get; set; }
    public Dictionary<string, string> FormData { get; set; }
    public string ObjectKey { get; set; }
    public string AvatarUrl { get; set; }
    public int ExpiresInSeconds { get; set; }
}