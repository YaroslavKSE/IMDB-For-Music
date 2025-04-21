using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.DTOs;
using MusicLists.Application.Results;
using MusicLists.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class UpdateMusicListUseCase : IRequestHandler<UpdateMusicListCommand, UpdateMusicListResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public UpdateMusicListUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<UpdateMusicListResult> Handle(UpdateMusicListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create domain entities
            var listItems = new List<ListItem>();
            foreach (var item in request.Items)
            {
                listItems.Add(new ListItem(item.SpotifyId, item.Number));
            }

            // Since we don't have a GetListById method, we'll create a new List with the provided ID
            // Note: In a real application, you might want to fetch the list first to verify it exists
            var list = new List(
                request.ListId,
                request.ListType,
                request.ListName,
                request.ListDescription,
                request.IsRanked,
                0, // Likes - would come from the database in a real implementation
                0, // Comments - would come from the database in a real implementation
                DateTime.UtcNow, // CreatedAt - would come from the database in a real implementation
                listItems
            );

            // Update in storage
            await _musicListsStorage.UpdateListAsync(list);

            return new UpdateMusicListResult
            {
                Success = true
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new UpdateMusicListResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new UpdateMusicListResult
            {
                Success = false,
                ErrorMessage = $"Error updating list: {ex.Message}"
            };
        }
    }
}