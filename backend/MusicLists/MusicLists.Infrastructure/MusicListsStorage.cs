using Microsoft.EntityFrameworkCore;
using MusicLists.Application;
using MusicLists.Domain;
using MusicLists.Infrastructure.DBConfig;
using MusicLists.Infrastructure.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

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
}