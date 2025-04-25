using MusicCatalogService.Core.DTOs;
using MusicCatalogService.Core.Helpers;
using MusicCatalogService.Core.Models;
using MusicCatalogService.Core.Spotify;

namespace MusicCatalogService.Core.Mappers;

/// <summary>
/// Maps artist data between different representations: Entity, DTO, and Spotify responses
/// </summary>
public static class ArtistMapper
{
    /// <summary>
    /// Maps an Artist entity to an ArtistDetailDto
    /// </summary>
    /// <param name="artist">The artist entity from the database</param>
    /// <returns>An ArtistDetailDto representation</returns>
    public static ArtistDetailDto MapArtistEntityToDto(Artist artist)
    {
        // Create image DTOs (we'll only have the thumbnail URL in the entity)
        var imageDtos = new List<ImageDto>();
        if (!string.IsNullOrEmpty(artist.ThumbnailUrl))
        {
            imageDtos.Add(new ImageDto
            {
                Url = artist.ThumbnailUrl,
                // These will be null since we only store the URL
                Height = null,
                Width = null
            });
        }
        
        // Create the ArtistDetailDto
        return new ArtistDetailDto
        {
            CatalogItemId = artist.Id,
            SpotifyId = artist.SpotifyId,
            Name = artist.Name,
            ImageUrl = artist.ThumbnailUrl,
            Images = imageDtos,
            Popularity = artist.Popularity,
            Genres = artist.Genres,
            FollowersCount = artist.FollowersCount ?? 0,
            ExternalUrls = [artist.SpotifyUrl],
            
            // Initialize empty lists for related content that would be populated by API calls
            TopAlbums = new List<AlbumSummaryDto>(),
            TopTracks = new List<TrackSummaryDto>()
        };
    }

    /// <summary>
    /// Maps a Spotify artist response to an ArtistDetailDto
    /// </summary>
    /// <param name="artist">The Spotify artist response</param>
    /// <param name="catalogItemId">The internal catalog ID</param>
    /// <returns>An ArtistDetailDto representation</returns>
    public static ArtistDetailDto MapToArtistDetailDto(SpotifyArtistResponse artist, Guid catalogItemId)
    {
        // Get all images (we'll still provide all available sizes in the DTO)
        var images = artist.Images.Select(img => new ImageDto
        {
            Url = img.Url,
            Height = img.Height,
            Width = img.Width
        }).ToList();

        return new ArtistDetailDto
        {
            CatalogItemId = catalogItemId,
            SpotifyId = artist.Id,
            Name = artist.Name,
            ImageUrl = ImageHelper.GetOptimalImage(artist.Images),
            Images = images,
            Popularity = artist.Popularity,
            Genres = artist.Genres,
            FollowersCount = artist.FollowersCount,
            ExternalUrls = new List<string> {artist.ExternalUrls.Spotify},
                
            // Initialize empty lists for related content
            TopAlbums = new List<AlbumSummaryDto>(),
            TopTracks = new List<TrackSummaryDto>()
        };
    }

    /// <summary>
    /// Maps a Spotify artist albums response to an ArtistAlbumsResultDto
    /// </summary>
    public static ArtistAlbumsResultDto MapToArtistAlbumsResultDto(
        SpotifyArtistAlbumsResponse response,
        string artistId,
        string artistName,
        int limit,
        int offset)
    {
        var result = new ArtistAlbumsResultDto
        {
            ArtistId = artistId,
            ArtistName = artistName,
            Limit = limit,
            Offset = offset,
            TotalResults = response.Total,
            Next = response.Next,
            Previous = response.Previous
        };

        // Map albums
        if (response.Items != null)
        {
            result.Albums = response.Items.Select(album => new AlbumSummaryDto
            {
                SpotifyId = album.Id,
                Name = album.Name,
                ArtistName = album.Artists.FirstOrDefault()?.Name ?? artistName,
                ReleaseDate = album.ReleaseDate,
                AlbumType = album.AlbumType,
                TotalTracks = album.TotalTracks,
                ImageUrl = ImageHelper.GetOptimalImage(album.Images),
                Images = album.Images?.Select(img => new ImageDto
                {
                    Url = img.Url,
                    Height = img.Height,
                    Width = img.Width
                }).ToList() ?? new List<ImageDto>(),
                ExternalUrls = album.ExternalUrls?.Spotify != null ? new List<string> { album.ExternalUrls.Spotify } : null
            }).ToList();
        }

        return result;
    }

    /// <summary>
    /// Maps a Spotify artist top tracks response to an ArtistTopTracksResultDto
    /// </summary>
    public static ArtistTopTracksResultDto MapToArtistTopTracksResultDto(
        SpotifyArtistTopTracksResponse response,
        string artistId,
        string artistName,
        string market)
    {
        var result = new ArtistTopTracksResultDto
        {
            ArtistId = artistId,
            ArtistName = artistName,
            Market = market
        };

        // Map tracks
        if (response.Tracks != null)
        {
            result.Tracks = response.Tracks.Select(track => new TrackSummaryDto
            {
                SpotifyId = track.Id,
                Name = track.Name,
                ArtistName = track.Artists.FirstOrDefault()?.Name ?? artistName,
                DurationMs = track.DurationMs,
                IsExplicit = track.Explicit,
                TrackNumber = track.TrackNumber,
                AlbumId = track.Album?.Id,
                ImageUrl = ImageHelper.GetOptimalImage(track.Album?.Images),
                Images = track.Album?.Images?.Select(img => new ImageDto
                {
                    Url = img.Url,
                    Height = img.Height,
                    Width = img.Width
                }).ToList() ?? new List<ImageDto>(),
                Popularity = track.Popularity,
                ExternalUrls = track.ExternalUrls?.Spotify != null ? new List<string> { track.ExternalUrls.Spotify } : null
            }).ToList();
        }

        return result;
    }

    /// <summary>
    /// Maps a SpotifyArtistResponse to an Artist entity
    /// </summary>
    /// <param name="spotifyArtist">The Spotify artist response</param>
    /// <param name="existingArtist">Optional existing artist to update</param>
    /// <returns>An Artist entity</returns>
    public static Artist MapToArtistEntity(SpotifyArtistResponse spotifyArtist, Artist? existingArtist = null)
    {
        var artistEntity = existingArtist ?? new Artist
        {
            Id = Guid.NewGuid(),
            TopTrackIds = new List<string>(),
            RelatedArtistIds = new List<string>(),
            AlbumIds = new List<string>()
        };

        // Preserve existing related data lists if available
        var topTrackIds = artistEntity.TopTrackIds;
        var relatedArtistIds = artistEntity.RelatedArtistIds;
        var albumIds = artistEntity.AlbumIds;

        // Update all the properties
        artistEntity.SpotifyId = spotifyArtist.Id;
        artistEntity.Name = spotifyArtist.Name;
        artistEntity.ArtistName = spotifyArtist.Name;
        artistEntity.ThumbnailUrl = ImageHelper.GetOptimalImage(spotifyArtist.Images);
        artistEntity.Popularity = spotifyArtist.Popularity;
        artistEntity.LastAccessed = DateTime.UtcNow;
        artistEntity.Genres = spotifyArtist.Genres;
        artistEntity.FollowersCount = spotifyArtist.FollowersCount;
        artistEntity.SpotifyUrl = spotifyArtist.ExternalUrls?.Spotify;
        
        // Restore the lists to preserve existing data
        artistEntity.TopTrackIds = topTrackIds;
        artistEntity.RelatedArtistIds = relatedArtistIds;
        artistEntity.AlbumIds = albumIds;

        return artistEntity;
    }
}