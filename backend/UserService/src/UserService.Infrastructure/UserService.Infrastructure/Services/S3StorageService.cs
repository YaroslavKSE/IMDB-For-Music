using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Configuration;

namespace UserService.Infrastructure.Services;

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AWSSettings _settings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<AWSSettings> settings,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadUserAvatarAsync(IFormFile file, Guid userId)
    {
        try
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            // Validate file type
            ValidateImageFile(file);

            // Generate a unique key for the avatar using the user ID
            var objectKey = $"avatars/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var fileStream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                BucketName = _settings.AvatarBucketName,
                Key = objectKey,
                CannedACL = S3CannedACL.PublicRead,
                ContentType = file.ContentType
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            // Build and return the URL
            var avatarUrl = $"{_settings.AvatarBaseUrl}/{objectKey}";
            _logger.LogInformation("Uploaded avatar for user {UserId} to {Url}", userId, avatarUrl);
            
            return avatarUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAvatarAsync(Guid userId)
    {
        try
        {
            // List all objects with the prefix for the user's avatars
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _settings.AvatarBucketName,
                Prefix = $"avatars/{userId}/"
            };

            var listResponse = await _s3Client.ListObjectsV2Async(listRequest);
            
            if (listResponse.KeyCount == 0)
            {
                _logger.LogInformation("No avatars found for user {UserId}", userId);
                return true; // Nothing to delete
            }

            // Delete each avatar
            foreach (var s3Object in listResponse.S3Objects)
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _settings.AvatarBucketName,
                    Key = s3Object.Key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation("Deleted avatar {Key} for user {UserId}", s3Object.Key, userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatars for user {UserId}", userId);
            return false;
        }
    }

    public async Task<PresignedUploadResponse> GeneratePresignedUploadUrlAsync(Guid userId, string contentType)
    {
        try
        {
            // Validate content type
            if (!IsValidImageContentType(contentType))
                throw new ArgumentException("Invalid image type", nameof(contentType));

            string extension = GetExtensionFromContentType(contentType);
            string objectKey = $"avatars/{userId}/{Guid.NewGuid()}{extension}";

            // Generate a presigned URL for direct browser uploads
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.AvatarBucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = DateTime.UtcNow.AddMinutes(15) // URL expires in 15 minutes
            };

            string url = _s3Client.GetPreSignedURL(request);
            
            // Calculate the full avatar URL after upload
            string avatarUrl = $"{_settings.AvatarBaseUrl}/{objectKey}";
            
            _logger.LogInformation("Generated presigned URL for user {UserId}", userId);

            return new PresignedUploadResponse
            {
                Url = url,
                FormData = null, // For PUT requests, no form data is needed
                ObjectKey = objectKey,
                AvatarUrl = avatarUrl,
                ExpiresInSeconds = 15 * 60 // 15 minutes in seconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for user {UserId}", userId);
            throw;
        }
    }

    #region Private Helper Methods

    private void ValidateImageFile(IFormFile file)
    {
        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size exceeds the maximum limit of 5MB", nameof(file));

        // Validate file type
        if (!IsValidImageContentType(file.ContentType))
            throw new ArgumentException("File type not supported. Only JPEG, PNG, and GIF are allowed", nameof(file));
    }

    private bool IsValidImageContentType(string contentType)
    {
        return contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("image/gif", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("image/heic", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("image/heif", StringComparison.OrdinalIgnoreCase);
    }

    private string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            _ => ".jpg" // Default to jpg
        };
    }

    #endregion
}