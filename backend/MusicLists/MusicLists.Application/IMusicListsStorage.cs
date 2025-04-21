namespace MusicLists.Application;

using Domain;

public interface IMusicListsStorage
{
    // Existing methods
    Task CreateListAsync(List list);
    Task UpdateListAsync(List list);
    Task DeleteListAsync(Guid listId);
    Task<ListLike> AddListLikeAsync(Guid listId, string userId);
    Task<bool> RemoveListLike(Guid listId, string userId);
    Task<bool> HasUserLikedList(Guid listId, string userId);
    Task<ListComment> AddListCommentAsync(Guid listId, string userId, string commentText);
    Task<bool> DeleteListComment(Guid commentId, string userId);

    // New methods
    Task<ListWithItemCount> GetListByIdAsync(Guid listId, int maxItems = 100);
    Task<PaginatedResult<ListItem>> GetListItemsByIdAsync(Guid listId, int? limit = null, int? offset = null);
    Task<PaginatedResult<ListComment>> GetListCommentsByIdAsync(Guid listId, int? limit = null, int? offset = null);
    Task<PaginatedResult<ListWithItemCount>> GetListsByUserIdAsync(string userId, int? limit = null, int? offset = null);
    Task<PaginatedResult<ListWithItemCount>> GetListsBySpotifyIdAsync(string spotifyId, int? limit = null, int? offset = null);
}

public class PaginatedResult<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }

    public PaginatedResult(List<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}