using Microsoft.EntityFrameworkCore;
using MusicLists.Application;
using MusicLists.Domain;
using MusicLists.Infrastructure.DBConfig;
using MusicLists.Infrastructure.Entities;

namespace MusicLists.Infrastructure;

public class MusicListsStorage : IMusicListsStorage
{
    private readonly MusicListsDbContext _dbContext;

    public MusicListsStorage(MusicListsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateListAsync(List list)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Create the list entity
            var listEntity = new ListEntity
            {
                ListId = list.ListId,
                UserId = list.UserId,
                ListType = list.ListType,
                CreatedAt = list.CreatedAt,
                ListName = list.ListName,
                ListDescription = list.ListDescription,
                IsRanked = list.IsRanked,
                IsScoreDirty = true, // Mark as dirty for hot score calculation
                HotScore = 0
            };

            // Add the list entity to the context
            await _dbContext.Lists.AddAsync(listEntity);

            // Create and add list items if any
            if (list.Items?.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    var listItemEntity = new ListItemEntity
                    {
                        ListItemId = Guid.NewGuid(),
                        ListId = list.ListId,
                        ItemId = item.SpotifyId,
                        Number = item.Number,
                        List = listEntity
                    };

                    await _dbContext.ListItems.AddAsync(listItemEntity);
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateListAsync(List list)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Find the existing list
            var existingList = await _dbContext.Lists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.ListId == list.ListId);

            if (existingList == null)
            {
                throw new KeyNotFoundException($"List with ID {list.ListId} not found.");
            }

            // Update list properties
            existingList.ListName = list.ListName;
            existingList.ListDescription = list.ListDescription;
            existingList.IsRanked = list.IsRanked;
            existingList.ListType = list.ListType;

            // Handle list items - remove all existing items and add new ones
            _dbContext.ListItems.RemoveRange(existingList.Items);

            // Add new items
            if (list.Items?.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    var listItemEntity = new ListItemEntity
                    {
                        ListItemId = Guid.NewGuid(),
                        ListId = list.ListId,
                        ItemId = item.SpotifyId,
                        Number = item.Number,
                        List = existingList
                    };

                    await _dbContext.ListItems.AddAsync(listItemEntity);
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteListAsync(Guid listId)
    {
        var list = await _dbContext.Lists.FindAsync(listId);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Due to cascade delete configuration in the DbContext,
        // deleting the list will automatically delete related items, likes, and comments
        _dbContext.Lists.Remove(list);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ListLike> AddListLikeAsync(Guid listId, string userId)
    {
        // Check if the list exists
        var list = await _dbContext.Lists.FindAsync(listId);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Check if the user has already liked this list
        var existingLike = await _dbContext.ListLikes
            .FirstOrDefaultAsync(l => l.ListId == listId && l.UserId == userId);

        if (existingLike != null)
        {
            throw new InvalidOperationException($"User {userId} has already liked list {listId}.");
        }

        // Create a new domain like object
        var like = new ListLike(listId, userId);

        // Create the entity
        var likeEntity = new ListLikeEntity
        {
            LikeId = like.LikeId,
            ListId = like.ListId,
            UserId = like.UserId,
            LikedAt = like.LikedAt
        };

        // Add to database
        await _dbContext.ListLikes.AddAsync(likeEntity);

        // Mark the list's hot score as dirty
        list.IsScoreDirty = true;

        await _dbContext.SaveChangesAsync();

        return like;
    }

    public async Task<bool> RemoveListLike(Guid listId, string userId)
    {
        // Find the like
        var like = await _dbContext.ListLikes
            .FirstOrDefaultAsync(l => l.ListId == listId && l.UserId == userId);

        if (like == null)
        {
            return false;
        }

        // Find the list to mark it as dirty
        var list = await _dbContext.Lists.FindAsync(listId);
        if (list != null)
        {
            list.IsScoreDirty = true;
        }

        // Remove the like
        _dbContext.ListLikes.Remove(like);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasUserLikedList(Guid listId, string userId)
    {
        return await _dbContext.ListLikes
            .AnyAsync(l => l.ListId == listId && l.UserId == userId);
    }

    public async Task<ListComment> AddListCommentAsync(Guid listId, string userId, string commentText)
    {
        // Check if the list exists
        var list = await _dbContext.Lists.FindAsync(listId);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Create the domain comment
        var comment = new ListComment(listId, userId, commentText);

        // Create the entity
        var commentEntity = new ListCommentEntity
        {
            CommentId = comment.CommentId,
            ListId = comment.ListId,
            UserId = comment.UserId,
            CommentedAt = comment.CommentedAt,
            CommentText = comment.CommentText
        };

        // Add to database
        await _dbContext.ListComments.AddAsync(commentEntity);

        // Mark the list's hot score as dirty
        list.IsScoreDirty = true;

        await _dbContext.SaveChangesAsync();

        return comment;
    }

    public async Task<bool> DeleteListComment(Guid commentId, string userId)
    {
        // Find the comment and ensure it belongs to the user
        var comment = await _dbContext.ListComments
            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);

        if (comment == null)
        {
            return false;
        }

        // Find the list to mark it as dirty
        var list = await _dbContext.Lists.FindAsync(comment.ListId);
        if (list != null)
        {
            list.IsScoreDirty = true;
        }

        // Remove the comment
        _dbContext.ListComments.Remove(comment);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<ListWithItemCount> GetListByIdAsync(Guid listId, int maxItems = 100)
    {
        var listEntity = await _dbContext.Lists
            .Include(l => l.Items.OrderBy(i => i.Number).Take(maxItems))
            .Include(l => l.Likes)
            .Include(l => l.Comments)
            .FirstOrDefaultAsync(l => l.ListId == listId);

        if (listEntity == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Count total items without loading them all
        var totalItems = await _dbContext.ListItems.CountAsync(i => i.ListId == listId);

        // Map items to domain objects
        var items = listEntity.Items
            .OrderBy(i => i.Number)
            .Select(i => new ListItem(i.ItemId, i.Number))
            .ToList();

        // Create the list domain object
        var list = new List(
            listEntity.ListId,
            listEntity.ListType,
            listEntity.ListName,
            listEntity.ListDescription,
            listEntity.IsRanked,
            listEntity.Likes.Count,
            listEntity.Comments.Count,
            listEntity.CreatedAt,
            items
        )
        {
            UserId = listEntity.UserId
        };

        // Return the enhanced list with item count
        return new ListWithItemCount(list, totalItems);
    }

    public async Task<PaginatedResult<ListItem>> GetListItemsByIdAsync(Guid listId, int? limit = null, int? offset = null)
    {
        if (!await _dbContext.Lists.AnyAsync(l => l.ListId == listId))
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Create query for list items
        IQueryable<ListItemEntity> query = _dbContext.ListItems
            .Where(i => i.ListId == listId)
            .OrderBy(i => i.Number);

        // Get total count efficiently
        int totalCount = await query.CountAsync();

        // Apply pagination
        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        // Execute the query
        var listItemEntities = await query.ToListAsync();

        // Map to domain objects
        var listItems = listItemEntities
            .Select(i => new ListItem(i.ItemId, i.Number))
            .ToList();

        // Return paginated result
        return new PaginatedResult<ListItem>(listItems, totalCount);
    }

    public async Task<PaginatedResult<ListComment>> GetListCommentsByIdAsync(Guid listId, int? limit = null, int? offset = null)
    {
        // Check if the list exists
        if (!await _dbContext.Lists.AnyAsync(l => l.ListId == listId))
        {
            throw new KeyNotFoundException($"List with ID {listId} not found.");
        }

        // Create query for list comments
        IQueryable<ListCommentEntity> query = _dbContext.ListComments
            .Where(c => c.ListId == listId)
            .OrderByDescending(c => c.CommentedAt);

        // Get total count efficiently
        int totalCount = await query.CountAsync();

        // Apply pagination
        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        // Execute the query
        var commentEntities = await query.ToListAsync();

        // Map to domain objects
        var comments = commentEntities
            .Select(c => new ListComment(
                c.CommentId,
                c.ListId,
                c.UserId,
                c.CommentedAt,
                c.CommentText))
            .ToList();

        // Return paginated result
        return new PaginatedResult<ListComment>(comments, totalCount);
    }

    public async Task<PaginatedResult<ListWithItemCount>> GetListsByUserIdAsync(string userId, int? limit = null, int? offset = null)
    {
        // Create query for lists by user ID
        IQueryable<ListEntity> query = _dbContext.Lists
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt);

        // Get total count efficiently
        int totalCount = await query.CountAsync();

        // Apply pagination
        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        query.Include(l => l.Likes)
            .Include(l => l.Comments);

        // Execute the query
        var listEntities = await query.ToListAsync();

        // Create the result lists
        List<ListWithItemCount> resultLists = new List<ListWithItemCount>();

        foreach (var listEntity in listEntities)
        {
            // For each list, get the first 5 items
            var previewItems = await _dbContext.ListItems
                .Where(i => i.ListId == listEntity.ListId)
                .OrderBy(i => i.Number)
                .Take(5)
                .Select(i => new ListItem(i.ItemId, i.Number))
                .ToListAsync();

            // Count total items for this list
            var totalItems = await _dbContext.ListItems
                .CountAsync(i => i.ListId == listEntity.ListId);

            // Create the list domain object
            var list = new List(
                listEntity.ListId,
                listEntity.ListType,
                listEntity.ListName,
                listEntity.ListDescription,
                listEntity.IsRanked,
                listEntity.Likes.Count,
                listEntity.Comments.Count,
                listEntity.CreatedAt,
                previewItems
            )
            {
                UserId = listEntity.UserId
            };

            // Add to the result
            resultLists.Add(new ListWithItemCount(list, totalItems));
        }

        // Return paginated result
        return new PaginatedResult<ListWithItemCount>(resultLists, totalCount);
    }

    public async Task<PaginatedResult<ListWithItemCount>> GetListsBySpotifyIdAsync(string spotifyId, int? limit = null, int? offset = null)
    {
        // First, find all list IDs that contain the spotify ID
        var listIds = await _dbContext.ListItems
            .Where(i => i.ItemId == spotifyId)
            .Select(i => i.ListId)
            .Distinct()
            .ToListAsync();

        // Create query for lists containing the spotify ID
        IQueryable<ListEntity> query = _dbContext.Lists
            .Where(l => listIds.Contains(l.ListId))
            .OrderByDescending(l => l.HotScore); // Sort by hot score

        // Get total count efficiently
        int totalCount = await query.CountAsync();

        // Apply pagination
        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        query.Include(l => l.Likes)
            .Include(l => l.Comments);

        // Execute the query
        var listEntities = await query.ToListAsync();

        // Create the result lists
        List<ListWithItemCount> resultLists = new List<ListWithItemCount>();

        foreach (var listEntity in listEntities)
        {
            // For each list, get the first 5 items
            var previewItems = await _dbContext.ListItems
                .Where(i => i.ListId == listEntity.ListId)
                .OrderBy(i => i.Number)
                .Take(5)
                .Select(i => new ListItem(i.ItemId, i.Number))
                .ToListAsync();

            // Count total items for this list
            var totalItems = await _dbContext.ListItems
                .CountAsync(i => i.ListId == listEntity.ListId);

            // Create the list domain object
            var list = new List(
                listEntity.ListId,
                listEntity.ListType,
                listEntity.ListName,
                listEntity.ListDescription,
                listEntity.IsRanked,
                listEntity.Likes.Count,
                listEntity.Comments.Count,
                listEntity.CreatedAt,
                previewItems
            )
            {
                UserId = listEntity.UserId
            };

            // Add to the result
            resultLists.Add(new ListWithItemCount(list, totalItems));
        }

        // Return paginated result
        return new PaginatedResult<ListWithItemCount>(resultLists, totalCount);
    }

    public async Task<int> InsertListItemAsync(Guid listId, string spotifyId, int position)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Check if the list exists
            var list = await _dbContext.Lists.FindAsync(listId);
            if (list == null)
            {
                throw new KeyNotFoundException($"List with ID {listId} not found.");
            }

            // Get the current items count and max position
            var existingItems = await _dbContext.ListItems
                .Where(i => i.ListId == listId)
                .ToListAsync();

            int maxPosition = 0;
            if (existingItems.Any())
            {
                maxPosition = existingItems.Max(i => i.Number);
            }

            // Validate the requested position
            if (position < 1)
            {
                position = 1; // If position is less than 1, insert at the beginning
            }
            else if (position > maxPosition + 1)
            {
                position = maxPosition + 1; // If position is beyond the end, append to the end
            }

            // Shift all items at and after the insertion position
            var itemsToShift = existingItems
                .Where(i => i.Number >= position)
                .OrderByDescending(i => i.Number) // Process from highest to lowest to avoid conflicts
                .ToList();

            foreach (var item in itemsToShift)
            {
                item.Number += 1;
            }

            // Create the new item
            var newItem = new ListItemEntity
            {
                ListItemId = Guid.NewGuid(),
                ListId = listId,
                ItemId = spotifyId,
                Number = position
            };

            await _dbContext.ListItems.AddAsync(newItem);

            // Save all changes
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return the total count of items after insertion
            return existingItems.Count + 1;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}