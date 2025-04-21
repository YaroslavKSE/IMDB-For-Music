using MusicLists.Domain;

namespace MusicLists.Application;

public interface IMusicListsStorage
{
    Task CreateListAsync(List list);
    Task UpdateListAsync(List list);
    Task DeleteListAsync(Guid listId);
    Task<ListLike> AddListLikeAsync(Guid listId, string userId);
    Task<bool> RemoveListLike(Guid listId, string userId);
    Task<bool> HasUserLikedList(Guid listId, string userId);
    Task<ListComment> AddListCommentAsync(Guid listId, string userId, string commentText);
    Task<bool> DeleteListComment(Guid commentId, string userId);
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