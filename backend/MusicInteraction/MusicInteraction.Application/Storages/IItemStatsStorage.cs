using MusicInteraction.Domain;

namespace MusicInteraction.Application.Interfaces;

public interface IItemStatsStorage
{
    Task<ItemStats> GetItemStatsAsync(string itemId);
    Task UpdateItemStatsAsync(string itemId);
    Task MarkItemStatsAsRawAsync(string itemId);
    Task ProcessAllRawItemStatsAsync();
    Task<bool> ItemStatsExistsAsync(string itemId);
    Task InitializeItemStatsAsync(string itemId);
}