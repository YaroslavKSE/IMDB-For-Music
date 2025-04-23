using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Helpers;

/// <summary>
/// Helper class for handling image-related operations
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Gets the optimal image URL from a list of Spotify images (640x640 or closest)
    /// </summary>
    /// <param name="images">List of Spotify images</param>
    /// <returns>URL of the optimal image or null if no images available</returns>
    public static string GetOptimalImage(List<SpotifyImage> images)
    {
        if (images == null || !images.Any())
            return null;

        // Try to find a 640x640 image
        var optimalImage = images.FirstOrDefault(img => img.Width == 640 && img.Height == 640);

        // If no 640x640 image exists, take the largest available
        if (optimalImage == null) 
            optimalImage = images.OrderByDescending(img => img.Width * img.Height).First();

        return optimalImage.Url;
    }
}