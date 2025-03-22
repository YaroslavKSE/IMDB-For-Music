﻿using MusicCatalogService.Core.DTOs;

namespace MusicCatalogService.Core.Interfaces;

public interface IAlbumService
{
    // Get album from Spotify and cache
    Task<AlbumDetailDto> GetAlbumAsync(string spotifyId);
    
    // Get album from internal database 
    Task<AlbumDetailDto> GetAlbumByCatalogIdAsync(Guid catalogId);
    
    // Save album into internal database 
    Task<AlbumDetailDto> SaveAlbumAsync(string spotifyId);
}